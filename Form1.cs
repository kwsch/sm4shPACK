using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace sm4shPACK
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(drag_enter);
            this.DragDrop += new DragEventHandler(drag_drop);
        }
        private void drag_enter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        private void drag_drop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            string path = files[0];
            for (int i = 0; i < files.Length; i++) 
                parse(files[i]);
        }
        private void parse(string path)
        {
            string newFolder = Path.Combine(Path.GetDirectoryName(path),Path.GetFileNameWithoutExtension(path));

            using (var s = new FileStream(path, FileMode.Open))
            {
                using (var br = new BinaryReader(s))
                {
                    uint magic = br.ReadUInt32();
                    if (magic != (0x4B434150)) return;

                    uint unk1 = br.ReadUInt32();
                    uint count = br.ReadUInt32();
                    uint unk2 = br.ReadUInt32();

                    uint[] fnoffsets = new uint[count];
                    uint[] filedatas = new uint[count];
                    uint[] datalength = new uint[count];
                    string[] filenames = new string[count];
                    Directory.CreateDirectory(newFolder);

                    for (int i = 0; i < count; i++)
                        fnoffsets[i] = br.ReadUInt32();
                    for (int i = 0; i < count; i++)
                        filedatas[i] = br.ReadUInt32();
                    for (int i = 0; i < count; i++)
                        datalength[i] = br.ReadUInt32();
                    for (int i = 0; i < count; i++)
                    {
                        br.BaseStream.Seek(fnoffsets[i],SeekOrigin.Begin);
                        string str = "";
                        char c = (char)br.ReadByte();
                        while (c != 0)
                        {
                            str += c;
                            c = (char)br.ReadByte();
                        }
                        filenames[i] = str;
                    }
                    for (int i = 0; i < count; i++)
                    {
                        br.BaseStream.Seek(filedatas[i], SeekOrigin.Begin);
                        using (MemoryStream dataout = new MemoryStream())
                        {
                            s.CopyTo(dataout, (int)datalength[i]);
                            if (filenames[i].Length == 0) filenames[i] = i.ToString() + ".bin";
                            byte[] newfile = dataout.ToArray(); Array.Resize(ref newfile, (int)datalength[i]);
                            File.WriteAllBytes(Path.Combine(newFolder, filenames[i]), newfile);
                        }
                    }

                }
            }
            System.Media.SystemSounds.Asterisk.Play();
        }
    }
}
