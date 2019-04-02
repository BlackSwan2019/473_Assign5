using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Assign5 {
    public partial class Form1 : Form {
        string filePath;
        string gameData;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            // Get file paths to each game.
            using (var file = new StreamReader("../../../Resources/directory.txt")) {

                // Read in each file path to each game.
                while ((filePath = file.ReadLine()) != null) {
                    // Open a single game's data file.
                    using (var gameFilepath = new StreamReader("../../../Resources/" + filePath)) {
                        // Read in game matrix data. Right now, I am reading it in as strings, when it should be a matrix of ints ultimately.
                        while ((gameData = gameFilepath.ReadLine()) != null) {
                            Console.WriteLine(gameData);
                        }

                        Console.WriteLine("\n");
                    }
                }
            }
        }
    }
}
