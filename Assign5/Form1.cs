﻿/*
 * Program:     Assignment 5
 * Author:      Patrick Klesyk, Ben Lane, Matt Rycraft
 * Z-ID:        Z1782152        Z1806979  Z1818053 
 * Description: A small logic game that mimics Soduku.
 * Due Date:    4/10/2019
 */

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
        string gameData;
        int numColumns;

        int[,] gameMatrix;

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            Dictionary<string, string> difficultyOptions = new Dictionary<string, string>();    // Holds game difficulty options for Difficulty dropdown menu.

            // Add options to the Difficulty dropdown menu data source.
            difficultyOptions.Add("easy/e1.txt", "Easy 1");
            difficultyOptions.Add("easy/e2.txt", "Easy 2");
            difficultyOptions.Add("easy/e3.txt", "Easy 3");

            // Set the dropdown menu to use the data source.
            comboBoxDifficulty.DataSource = new BindingSource(difficultyOptions, null);
            comboBoxDifficulty.DisplayMember = "Value";
            comboBoxDifficulty.ValueMember = "Key";
        }

        private void button1_Start(object sender, EventArgs e) {
            int row = 0,
                column = 0;

            using (var gameFile = new StreamReader("../../../Resources/" + ((KeyValuePair<string, string>)comboBoxDifficulty.SelectedItem).Key)) {
                gameData = gameFile.ReadLine();

                // Read a row of numbers from the file and count how many numbers there are. That's how many columns and rows the game will have.
                numColumns = (gameData.ToCharArray().Length);

                // Get a character array of that row's numbers.
                char[] charNums = gameData.ToCharArray();

                // Create the two-dimensional array that will hold game numbers.
                gameMatrix = new int[numColumns, numColumns];

                // Fill in the first row of the matrix with numbers.
                for (int i = 0; i < numColumns; i++) {
                    gameMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                }

                // Move on to next row.
                row++;

                while ((gameData = gameFile.ReadLine()) != null) {
                    // Get a character array of that row's numbers.
                    charNums = gameData.ToCharArray();

                    for (int i = 0; i < numColumns; i++) {
                        // Convert number character to integer and then insert into the game matrix.
                        gameMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                    }

                    // Move on to next row.
                    row++;

                    if (row == numColumns)
                        break;
                }

                Console.WriteLine("Initial state: ");

                // Print out initial game state.
                for (int r = 0; r < numColumns; r++) {
                    for (int c = 0; c < numColumns; c++) {
                        Console.Write(gameMatrix[r, c]);
                    }
                }

                // Create the two-dimensional array that will hold game numbers when solved.
                int[,] gameSolvedMatrix = new int[numColumns, numColumns];

                row = 0;

                while ((gameData = gameFile.ReadLine()) != null) {
                    // If the blank line between matrices is read in, skip processing it.
                    if (gameData.Length == 0) {
                        continue;
                    } 

                    // Get a character array of that row's numbers.
                    charNums = gameData.ToCharArray();

                    for (int i = 0; i < numColumns; i++) {
                        // Convert number character to integer and then insert into the game matrix.
                        gameSolvedMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                    }
                    
                    // Move on to next row.
                    row++;

                    if (row == numColumns)
                        break;
                }
                
                Console.WriteLine("\nFinal state: ");

                // Print out initial game state.
                for (int r = 0; r < numColumns; r++) {
                    for (int c = 0; c < numColumns; c++) {
                        Console.Write(gameSolvedMatrix[r, c]);
                    }
                }

                row = 0;
                
                Console.WriteLine("\n");

                drawGame();
            }
        }

        private void drawGame() {
            TextBox[] textBoxes = new TextBox[numColumns * numColumns];

            int x = 200;
            int y = 30;

            int row = 0;
            int col = 0;
            
            for (int i = 0; i < numColumns * numColumns; i++) {
                Console.WriteLine("ROW: " + row);
                // If we drew out the amount of columns we need, drop to new row.
                if (i % numColumns == 0 && i > 0) {
                    y += 40;
                    x = 200;
                    row++;
                    col = 0;
                }

                textBoxes[i] = new TextBox();

                // Set properties of the textBox.
                textBoxes[i].Multiline = true;
                textBoxes[i].Font = new Font(textBoxes[i].Font.FontFamily, 30);
                textBoxes[i].Location = new System.Drawing.Point(x, y);
                textBoxes[i].Height = 45;
                textBoxes[i].Width = 45;
                textBoxes[i].MaxLength = 2;
                textBoxes[i].Text = gameMatrix[row, col].ToString();

                // Add textBox to the form.
                Controls.Add(textBoxes[i]);

                // Shift right for next textBox placement.
                x += 40;

                // Go to next column.
                col++;
            }
        }
    }
}
