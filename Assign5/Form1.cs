﻿/*
 * Program:     Assignment 5
 * Author:      Patrick Klesyk, Ben Lane, Matt Rycraft
 * Z-ID:        Z1782152        Z1806979  Z1818053 
 * Description: A small logic game that mimics Soduku.
 * Due Date:    4/11/2019
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Timers;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace Assign5 {
    public partial class Form1 : Form {
        string gameData;    // A singe line of data from file.
        int numColumns;     // Number of columns in the field (same as rows).
        Dictionary<string, string> difficultyOptions = new Dictionary<string, string>();    // Holds game difficulty options for Difficulty dropdown menu.
        string fileName;        // Name of game file.
        string pathToGame;      // Directory path to gameFile.
        string completionTime;  // Time player took to complete a game.
        List<string> completionTimes = new List<string>();  // A list of times for completed games.

        System.Timers.Timer timer = new System.Timers.Timer(1000);  // Keeps track of game elapsed time.
        bool gameIsGoing = false;       // Whether game is in progress.
        static int hours = 0;           // Elapsed hours.
        static int minutes = 0;         // Elapsed minutes.
        static int seconds = 0;         // Elapsed seconds.

        bool savedGame = false;         // Whether game being played is from a save file.

        GameCell[] textBoxes;           // Game cells.

        Label[] sumsX;                  // Column sums.
        Label[] sumsY;                  // Row sums.
        Label sumsLeftDiag;             // Left diagonal sum.
        Label sumsRightDiag;            // Right diagonal sum.

        Label[] sumsXAnswer;            // Solution sum for columns.
        Label[] sumsYAnswer;            // Solution sum for rows.
        Label sumsLeftDiagAnswer;       // Solution sum for left diagonal.
        Label sumsRightDiagAnswer;      // Solution sum for right diagonal.

        int[] summationX;               // Column sums
        int[] summationY;               // Row sums
        int summationLeftDiag;          // Left diagonal sums.
        int summationRightDiag;         // Right diagonal sums.
        int[] summationXAnswer;         // Solution sum for columns.
        int[] summationYAnswer;         // Solution sum for rows.
        int summationLeftDiagAnswer;    // Solution sum for left diagonal.    
        int summationRightDiagAnswer;   // Solution sum for right diagonal.

        PictureBox blind = new PictureBox();    // Covers game when game is paused.

        // Import OS library for hiding caret in textboxes.
        [DllImport("user32.dll")]
        static extern bool HideCaret(System.IntPtr hWnd);

        int[,] initialMatrix;   // Data model for the game. It changes as user plays the game.
        int[,] gameMatrix;      // Data model for the game. It changes as user plays the game.
        int[,] solutionMatrix;  // Data model for the game. It changes as user plays the game.

        public Form1() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {
            radioButtonEasy.Checked = true;
        }

        /*  
        *  Method:     button1_Start
        *  
        *  Purpose:    Handles when user clicks the start button. Initializes data models. 
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        private void button1_Start(object sender, EventArgs e) {
            int row = 0;    // Roow of matrix.

            // Enable various buttons upon start.
            buttonSave.Enabled = true;
            buttonHelp.Enabled = true;
            buttonHelp.Enabled = true;
            buttonPause.Enabled = true;
            savedGame = false;

            // Clear and set message box.
            richTextMessages.Clear();
            FontFamily fontFamily = new FontFamily("Courier New");
            richTextMessages.Font = new Font(fontFamily, 11);

            // If there are textBoxes from last session, remove them.
            if (textBoxes != null) {
                for (int i = 0; i < numColumns * numColumns; i++) {
                    Controls.Remove(textBoxes[i]);
                }
            }

            // If there are labels from last session, remove them.
            if (sumsX != null) {
                for (int i = 0; i < numColumns; i++) {
                    Controls.Remove(sumsX[i]);
                    Controls.Remove(sumsY[i]);
                    Controls.Remove(sumsXAnswer[i]);
                    Controls.Remove(sumsYAnswer[i]);
                    Controls.Remove(sumsRightDiag);
                    Controls.Remove(sumsLeftDiag);
                    Controls.Remove(sumsLeftDiagAnswer);
                    Controls.Remove(sumsRightDiagAnswer);
                }
            }

            // Get file name according to combobox selection.
            fileName = ((KeyValuePair<string, string>)comboBoxGame.SelectedItem).Key;
            
            // If there is a saved game for this difficulty and field, then use that one.
            if ((new FileInfo("../../../Saves/" + fileName)).Exists) {
                pathToGame = "../../../Saves/" + fileName;
                savedGame = true;
            
                // Strip off old tick event.
                timer.Elapsed -= MyTimedEvent;
            } else {
                pathToGame = "../../../Resources/" + fileName;

                // It is a new game, so start timer at 0 seconds.
                hours = 0;
                minutes = 0;
                seconds = 0;

                // Strip off old tick event.
                timer.Elapsed -= MyTimedEvent;

                string timerElapsed = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);

                labelTimer.Text = timerElapsed;
            }

            using (var gameFile = new StreamReader(pathToGame)) {
                gameData = gameFile.ReadLine();

                // Read a row of numbers from the file and count how many numbers there are. That's how many columns and rows the game will have.
                numColumns = (gameData.ToCharArray().Length);

                // Get a character array of that row's numbers.
                char[] charNums = gameData.ToCharArray();

                initialMatrix = new int[numColumns, numColumns];

                if (savedGame) {
                    // Fill in the first row of the initial matrix with numbers.
                    for (int i = 0; i < numColumns; i++) {
                        initialMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                    }

                    // Move on to next row.
                    row++;

                    while ((gameData = gameFile.ReadLine()) != null) {
                        // Get a character array of that row's numbers.
                        charNums = gameData.ToCharArray();

                        for (int i = 0; i < numColumns; i++) {
                            // Convert number character to integer and then insert into the game matrix.
                            initialMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                        }

                        // Move on to next row.
                        row++;

                        if (row == numColumns)
                            break;
                    }
                }

                // Create the two-dimensional array that will hold game numbers.
                gameMatrix = new int[numColumns, numColumns];

                row = 0;

                // If this is a new game, then allow priming read for gameMatrix.
                if (!savedGame) {
                    // Fill in the first row of the matrix with numbers.
                    for (int i = 0; i < numColumns; i++) {
                        if (!savedGame) {
                            initialMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                        }

                        // Convert number character to integer and then insert into the game matrix.
                        gameMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                    }

                    // Move on to next row.
                    row++;
                }

                while ((gameData = gameFile.ReadLine()) != null) {
                    charNums = gameData.ToCharArray();

                    // If the blank line between matrices is read in, skip processing it.
                    if (gameData.Length == 0) {
                        continue;
                    }

                    // Get a character array of that row's numbers.
                    charNums = gameData.ToCharArray();

                    for (int i = 0; i < numColumns; i++) {
                        if (!savedGame) {
                            initialMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                        }

                        // Convert number character to integer and then insert into the game matrix.
                        gameMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                    }

                    // Move on to next row.
                    row++;

                    if (row == numColumns)
                        break;
                }

                // Create the two-dimensional array that will hold game numbers when solved.
                solutionMatrix = new int[numColumns, numColumns];

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
                        solutionMatrix[row, i] = (int)Char.GetNumericValue(charNums[i]);
                    }
                    
                    // Move on to next row.
                    row++;

                    if (row == numColumns)
                        break;
                }

                // Read in time elapsed for saved game.
                while ((gameData = gameFile.ReadLine()) != null) {
                    if (gameData.Length == 0) {
                        continue;
                    }

                    // Tokenize timer string.
                    string[] timestamp = gameData.Split(':');

                    hours = Convert.ToInt32(timestamp[0]);
                    minutes = Convert.ToInt32(timestamp[1]);
                    seconds = Convert.ToInt32(timestamp[2]);

                    // Set timer label to elapsed value.
                    string timerElapsed = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
                    labelTimer.Text = timerElapsed;
                }

                

                row = 0;

                getAnswers();
                drawGame();
            }
        }

        /*  
        *  Method:     getAnswers
        *  
        *  Purpose:    Get solution sums for all axes' data models. 
        * 
        *  Arguments:  none
        *              
        *  Return:     void
        */
        private void getAnswers() {
            summationXAnswer = new int[numColumns];
            summationYAnswer = new int[numColumns];

            summationLeftDiagAnswer = 0;
            summationRightDiagAnswer = 0;

            // Draw sum labels across the top of the play field.
            for (int i = 0; i < numColumns; i++) {
                // Add up column values.
                for (int r = 0; r < numColumns; r++) {
                    for (int c = 0; c < numColumns; c++) {
                        if (c == i) {
                            summationXAnswer[i] += solutionMatrix[r, c];
                        }
                    }
                }
            }

            // Draw sum labels down the left side the play field.
            for (int i = 0; i < numColumns; i++) {
                // Add up row values.
                for (int c = 0; c < numColumns; c++) {
                    for (int r = 0; r < numColumns; r++) {
                        if (r == i) {
                            summationYAnswer[i] += solutionMatrix[r, c];
                        }
                    }
                }
            }

            // Add up left diagonal from solution matrix.
            for (int r = 0; r < numColumns; r++) {
                for (int c = 0; c < numColumns; c++) {
                    if (c == r) {
                        summationLeftDiagAnswer += solutionMatrix[r, c];
                    }
                }
            }

            // Add up right diagonal from solution matrix.
            for (int r = 0, c = numColumns - 1; r < numColumns && c >= 0; r++, c--) {
                summationRightDiagAnswer += solutionMatrix[r, c];
            }
        }

        /*  
        *  Method:     drawGame
        *  
        *  Purpose:    Draws game field. 
        * 
        *  Arguments:  none
        *              
        *  Return:     void
        */
        private void drawGame() {
            textBoxes = new GameCell[numColumns * numColumns];  // A single game cell which player enters a value.

            sumsX = new Label[numColumns];  // Column running-sum labels.
            sumsY = new Label[numColumns];  // Row running-sum labels.

            summationX = new int[numColumns];   // Data model for column sums.
            summationY = new int[numColumns];   // Data model for row sums.

            summationLeftDiag = 0;      // Data model for left diagonal sum.
            summationRightDiag = 0;     // Data model for right diagonal sum.

            rowColTag rowCol;           // Tag for text box containing row and column indices.

            // Location of textboxes.
            int x = 300;    
            int y = 60;

            // Game matrix indices.
            int row = 0;
            int col = 0;

            // If there are textBoxes from last session, remove them.
            if (textBoxes != null) {
                for (int i = 0; i < numColumns * numColumns; i++) {
                    Controls.Remove(textBoxes[i]);
                }
            }

            for (int i = 0; i < numColumns * numColumns; i++) {
                // If we drew out the amount of columns we need, drop to new row.
                if (i % numColumns == 0 && i > 0) {
                    y += 385 / numColumns;

                    x = 300;
                    row++;
                    col = 0;
                }

                textBoxes[i] = new GameCell();

                // Set properties of the textBox.
                textBoxes[i].Multiline = true;

                // Set cell font size according to cell size.
                if (gameMatrix.Length == 9) {
                    textBoxes[i].Font = new Font(textBoxes[i].Font.FontFamily, 80);
                } else if (gameMatrix.Length == 25) {
                    textBoxes[i].Font = new Font(textBoxes[i].Font.FontFamily, 50);
                } else {
                    textBoxes[i].Font = new Font(textBoxes[i].Font.FontFamily, 35);
                }

                // Set textBox properties.
                textBoxes[i].Location = new Point(x, y);
                textBoxes[i].SelectionStart = 0;
                textBoxes[i].SelectionLength = textBoxes[i].Text.Length;
                textBoxes[i].Cursor = Cursors.Default;
                textBoxes[i].Height = 385 / numColumns;
                textBoxes[i].Width = 385 / numColumns;
                textBoxes[i].TextAlign = HorizontalAlignment.Center;
                textBoxes[i].MaxLength = 1;

                // Assign a row-and-column tag to this box so we can reference it later.
                rowCol = new rowColTag(row, col);
                textBoxes[i].Tag = rowCol;

                // If initialMatrix has a 0 at this element (meaning it was blank to begin with)...
                if (initialMatrix[row, col] == 0) {
                    // If we are in a saved game, put saved value in cell.
                    if (savedGame) {
                        if (gameMatrix[row, col] == 0) {
                            textBoxes[i].Text = "";
                        } else {
                            textBoxes[i].Text = gameMatrix[row, col].ToString();
                        }
                    } else { // Else, if user never put a value in there, leave it blank.
                        textBoxes[i].Text = "";
                    }
                } else {
                    textBoxes[i].Text = gameMatrix[row, col].ToString();
                    textBoxes[i].ReadOnly = true;
                    textBoxes[i].Enabled = false;

                    // If cell is disabled, don't use that "disabled look". Maintain a solid border.
                    textBoxes[i].BorderStyle = BorderStyle.FixedSingle;
                    textBoxes[i].BackColor = Color.FromArgb(230, 230, 230);
                }

                // Add onChange handler  to the textBox.
                textBoxes[i].TextChanged += valueChanged;

                // Add textBox to the form.
                Controls.Add(textBoxes[i]);

                // Shift right for next textBox placement.
                x += 385 / numColumns;

                // Go to next column.
                col++;
            }


            // Beginning coordinates of first top label.
            int topSumsX = 300 + (textBoxes[0].Width / 2) -  10;
            int topSumsY = 28;

            // Draw sum labels across the top of the play field.
            for (int i = 0; i < numColumns; i++) {
                sumsX[i] = new Label();

                // Add up column values.
                for (int r = 0; r < numColumns; r++) {
                    for (int c = 0; c < numColumns; c++) {
                        if (c == i) {
                            summationX[i] += gameMatrix[r, c];
                        }
                    }
                }

                // Set column running-sum label properties.
                sumsX[i].Text = summationX[i].ToString();
                sumsX[i].ForeColor = Color.White;
                sumsX[i].Height = 30;
                sumsX[i].Width = 30;
                sumsX[i].Location = new Point(topSumsX, topSumsY);
                sumsX[i].Font = new Font(sumsX[i].Font.FontFamily, 12);
                sumsX[i].TextAlign = ContentAlignment.MiddleCenter;

                Controls.Add(sumsX[i]);

                topSumsX += textBoxes[0].Width - 2;
            }

            // Beginning coordinates of first side label.
            int leftSumsX = 270;
            int leftSumsY = 45 + (textBoxes[0].Height / 2);

            // Draw sum labels down the left side the play field.
            for (int i = 0; i < numColumns; i++) {
                sumsY[i] = new Label();

                // Add up row values.
                for (int c = 0; c < numColumns; c++) {
                    for (int r = 0; r < numColumns; r++) {
                        if (r == i) {
                            summationY[i] += gameMatrix[r, c];
                        }
                    }
                }

                // Set row running-sum labels.
                sumsY[i].Text = summationY[i].ToString();
                sumsY[i].ForeColor = Color.White;
                sumsY[i].Height = 30;
                sumsY[i].Width = 30;
                sumsY[i].Location = new Point(leftSumsX, leftSumsY);
                sumsY[i].Font = new Font(sumsY[i].Font.FontFamily, 12);
                sumsY[i].TextAlign = ContentAlignment.MiddleCenter;

                Controls.Add(sumsY[i]);

                leftSumsY += textBoxes[0].Height - 1;
            }

            // Place left diagonal sum label.
            sumsLeftDiag = new Label();
            
            int leftDiagX = 270;
            int leftDiagY = 28;

            // Add up left diagonal values.
            for (int c = 0; c < numColumns; c++) {
                for (int r = 0; r < numColumns; r++) {
                    if (c == r) {
                        summationLeftDiag += gameMatrix[r, c];
                    }
                }
            }

            // Set properties of left diagonal running-sum label. 
            sumsLeftDiag.Text = summationLeftDiag.ToString();
            sumsLeftDiag.ForeColor = Color.White;
            sumsLeftDiag.Height = 30;
            sumsLeftDiag.Width = 30;
            sumsLeftDiag.Location = new Point(leftDiagX, leftDiagY);
            sumsLeftDiag.Font = new Font(sumsLeftDiag.Font.FontFamily, 12);
            sumsLeftDiag.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(sumsLeftDiag);

            // Place right diagonal sum label.
            sumsRightDiag = new Label();
            int rightDiagX = 680;
            int rightDiagY = 28;

            // Add up left diagonal values.
            for (int r = 0, c = numColumns - 1; r < numColumns && c >= 0; r++, c--) {
                summationRightDiag += gameMatrix[r, c];
            }

            // Set properties of right diagonal running-sum label.
            sumsRightDiag.Text = summationRightDiag.ToString();
            sumsRightDiag.ForeColor = Color.White;
            sumsRightDiag.Height = 30;
            sumsRightDiag.Width = 30;
            sumsRightDiag.Location = new Point(rightDiagX, rightDiagY);
            sumsRightDiag.Font = new Font(sumsRightDiag.Font.FontFamily, 12);
            sumsRightDiag.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(sumsRightDiag);

            if (savedGame) {
                setLabelColors();
            }

            // Place solution labels on the game field.
            drawSolutionLabels();

            // Start timing.
            beginTimer();
        }

        /*  
        *  Method:     drawSolutionLabels
        *  
        *  Purpose:    Draws correct-solution labels for columsn, rows, and diagonals. 
        * 
        *  Arguments:  none
        *              
        *  Return:     void
        */
        private void drawSolutionLabels() {
            // Answer labels for rows and columns.
            sumsXAnswer = new Label[numColumns];
            sumsYAnswer = new Label[numColumns];

            // Beginning coordinates of first top label.
            int topXAnswer = 295 + (textBoxes[0].Width / 2) - 10;
            int topYAnswer = 0;

            // Draw sum labels across the top of the play field.
            for (int i = 0; i < numColumns; i++) {
                sumsXAnswer[i] = new Label();
                sumsXAnswer[i].Text = "(" + summationXAnswer[i].ToString() + ")";
                sumsXAnswer[i].ForeColor = Color.White;
                sumsXAnswer[i].Height = 35;
                sumsXAnswer[i].Width = 40;
                sumsXAnswer[i].Location = new Point(topXAnswer, topYAnswer);
                sumsXAnswer[i].Font = new Font(sumsXAnswer[i].Font.FontFamily, 12);
                sumsXAnswer[i].TextAlign = ContentAlignment.MiddleCenter;
                Controls.Add(sumsXAnswer[i]);
                topXAnswer += textBoxes[0].Width - 1;
            }

            // Beginning coordinates of first side label.
            int leftSumsXAnswer = 235;
            int leftSumsYAnswer = 43 + (textBoxes[0].Height / 2);

            // Draw sum labels down the left side the play field.
            for (int i = 0; i < numColumns; i++) {
                sumsYAnswer[i] = new Label();
                sumsYAnswer[i].Text = "(" + summationYAnswer[i].ToString() + ")";
                sumsYAnswer[i].ForeColor = Color.White;
                sumsYAnswer[i].Height = 35;
                sumsYAnswer[i].Width = 40;
                sumsYAnswer[i].Location = new Point(leftSumsXAnswer, leftSumsYAnswer);
                sumsYAnswer[i].Font = new Font(sumsYAnswer[i].Font.FontFamily, 12);
                sumsYAnswer[i].TextAlign = ContentAlignment.MiddleCenter;
                Controls.Add(sumsYAnswer[i]);
                leftSumsYAnswer += textBoxes[0].Height - 1;
            }

            // Beginning coordinates of first top label.
            int leftDiagX = 235;
            int leftDiagY = 0;


            // Draw sum labels across the top of the play field.
            sumsLeftDiagAnswer = new Label();

            // Set properties of left diagonal solution label.
            sumsLeftDiagAnswer.Text = "(" + summationLeftDiagAnswer.ToString() + ")";
            sumsLeftDiagAnswer.ForeColor = Color.White;
            sumsLeftDiagAnswer.Height = 35;
            sumsLeftDiagAnswer.Width = 40;
            sumsLeftDiagAnswer.Location = new Point(leftDiagX, leftDiagY);
            sumsLeftDiagAnswer.Font = new Font(sumsLeftDiagAnswer.Font.FontFamily, 12);
            sumsLeftDiagAnswer.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(sumsLeftDiagAnswer);

            // Beginning coordinates of first top label.
            int rightDiagX = 700;
            int rightDiagY = 0;

            // Draw sum labels across the top of the play field.
            sumsRightDiagAnswer = new Label();
            sumsRightDiagAnswer.Text = "(" + summationRightDiagAnswer.ToString() + ")";
            sumsRightDiagAnswer.ForeColor = Color.White;
            sumsRightDiagAnswer.Height = 35;
            sumsRightDiagAnswer.Width = 40;
            sumsRightDiagAnswer.Location = new Point(rightDiagX, rightDiagY);
            sumsRightDiagAnswer.Font = new Font(sumsRightDiagAnswer.Font.FontFamily, 12);
            sumsRightDiagAnswer.TextAlign = ContentAlignment.MiddleCenter;

            Controls.Add(sumsRightDiagAnswer);

        }

        /*  
        *  Method:     beginTimer
        *  
        *  Purpose:    Starts the time-elapsed timer. 
        * 
        *  Arguments:  none
        *              
        *  Return:     void
        */
        private void beginTimer() {
            gameIsGoing = true;

            timer.AutoReset = true;
            timer.Elapsed += MyTimedEvent;
            timer.Enabled = true;
            timer.Start();
        }

        /*  
        *  Method:     MyTimedEvent
        *  
        *  Purpose:    Engages for every tick of the timer.
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        private void MyTimedEvent(object source, ElapsedEventArgs args) {
            //System.Timers.Timer timer = (System.Timers.Timer)source;
            seconds += 1;

            // Do math to get correct timestap of HH:MM:SS from an amount of seconds.
            if (seconds == 60) {
                minutes++;
                seconds = 0;
            }

            if (minutes == 60) {
                hours++;
                minutes = 0;
            }

            // Set timer label text.
            string timerElapsed = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
            labelTimer.Text = timerElapsed;
        }

        /*  
        *  Method:     buttonPause_Click
        *  
        *  Purpose:    Handles when user clicks the pause button. 
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        private void buttonPause_Click(object sender, EventArgs e) {
            // Set pause blinder properties.
            blind.Location = new Point(240, 0);
            blind.Width = 595;
            blind.Height = 500;
            blind.BackColor = Color.Red;
            blind.Paint += drawPauseMessage;

            // If game is going, then pause timer and cover playfield.
            if (gameIsGoing) {
                gameIsGoing = false;

                buttonPause.Text = "Resume";

                timer.Stop();

                // Blank out playfield.
                Controls.Add(blind);

                blind.BringToFront();
            } else { // Else, remove blinder and resume timer.
                gameIsGoing = true;

                buttonPause.Text = "Pause";

                // Remove blind.
                Controls.Remove(blind);

                timer.Start();
            }
        }

        /*  
        *  Method:     drawPauseMessage
        *  
        *  Purpose:    Draw the graphic over the playfield to obscure it when player pauses game. 
        * 
        *  Arguments:  object               UI component sending event.
        *              PaintEventArgs       The event.
        *              
        *  Return:     void
        */
        private void drawPauseMessage(object sender, PaintEventArgs e) {
            // Create string to draw.
            String drawString = "GAME PAUSED";

            // Create font and brush.
            Font drawFont = new Font("Arial", 16);
            SolidBrush drawBrush = new SolidBrush(Color.White);

            // Create point for upper-left corner of drawing.
            float x = 170;
            float y = 230.0F;

            e.Graphics.DrawString(drawString, drawFont, drawBrush, x, y);
        }

        /*  
        *  Method:     radioButtonEasy_CheckedChanged
        *  
        *  Purpose:    Check if easy difficulty was selected. 
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        private void radioButtonEasy_CheckedChanged(object sender, EventArgs e) {
            // Reset comboBox and enable it.
            difficultyOptions.Clear();
            comboBoxGame.Enabled = true;

            // Place options into comboBox.
            difficultyOptions.Add("easy/e1.txt", "Easy 1");
            difficultyOptions.Add("easy/e2.txt", "Easy 2");
            difficultyOptions.Add("easy/e3.txt", "Easy 3");

            // Set the dropdown menu to use the data source.
            comboBoxGame.DataSource = new BindingSource(difficultyOptions, null);
            comboBoxGame.DisplayMember = "Value";
            comboBoxGame.ValueMember = "Key";
        }

        /*  
        *  Method:     radioButtonMedium_CheckedChanged
        *  
        *  Purpose:    Checks whether medium difficulty was selected.
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        private void radioButtonMedium_CheckedChanged(object sender, EventArgs e) {
            // Reset comboBox and enable it.
            difficultyOptions.Clear();
            comboBoxGame.Enabled = true;
            
            // Populate comboBox.
            difficultyOptions.Add("medium/m1.txt", "Medium 1");
            difficultyOptions.Add("medium/m2.txt", "Medium 2");
            difficultyOptions.Add("medium/m3.txt", "Medium 3");

            // Set the dropdown menu to use the data source.
            comboBoxGame.DataSource = new BindingSource(difficultyOptions, null);
            comboBoxGame.DisplayMember = "Value";
            comboBoxGame.ValueMember = "Key";
        }

        /*  
        *  Method:     radioButtonHard_CheckedChanged
        *  
        *  Purpose:    Hard difficulty radio button handler. 
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        private void radioButtonHard_CheckedChanged(object sender, EventArgs e) {
            // Reset comboBox and enable it.
            difficultyOptions.Clear();
            comboBoxGame.Enabled = true;
            
            // Populate comboBox with options.
            difficultyOptions.Add("hard/h1.txt", "Hard 1");
            difficultyOptions.Add("hard/h2.txt", "Hard 2");
            difficultyOptions.Add("hard/h3.txt", "Hard 3");

            // Set the dropdown menu to use the data source.
            comboBoxGame.DataSource = new BindingSource(difficultyOptions, null);
            comboBoxGame.DisplayMember = "Value";
            comboBoxGame.ValueMember = "Key";
        }

        /*  
        *  Method:     valueChanged
        *  
        *  Purpose:    Handles when a user enters a value in a cell. When cell value changes, this will update
        *              row/column/diagonal sums. 
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        void valueChanged(object sender, EventArgs e) {
            // Convert sent-in object to GameCell.
            GameCell cell = (GameCell)sender;
            rowColTag rowCol = (rowColTag)cell.Tag;

            // Tokenize textBox content.
            char[] textBoxChars = (cell.Text).ToCharArray();

            // If cell is blank, then set its value to 0, else update gameMatrix with cell's new value.
            if (cell.Text.Length == 0) {
                gameMatrix[rowCol.row, rowCol.col] = 0;
            } else {
                // Update gameMatrix at the specified element.
                gameMatrix[rowCol.row, rowCol.col] = Convert.ToInt32(cell.Text);
            }

            setLabelColors();

            // If game is finished, then set varous UI element states.
            if (gameComplete()) {
                timer.Stop();

                buttonHelp.Enabled = false;
                buttonPause.Enabled = false;

                foreach (TextBox textBox in textBoxes) {
                    textBox.Enabled = false;
                }

                richTextMessages.Clear();

                // Display Win message.
                richTextMessages.AppendText("You win! \nDifficulty Times: \n");
                richTextMessages.AppendText("This game:  " + labelTimer.Text);

                // Write out win time to file.
                string[] fileStuff = fileName.Split('/');

                // If file doesn't exit, make it!
                if (!(new FileInfo("../../../Completed/" + fileStuff[0] + "/")).Exists) {
                    (new FileInfo("../../../Completed/" + fileStuff[0] + "/")).Directory.Create();
                }

                // Write out time to file.
                using (StreamWriter saveFile = File.AppendText("../../../Completed/" + fileStuff[0] + "/times.txt")) {
                    saveFile.WriteLine(labelTimer.Text);
                }

                // Get average time here.
                string[] filePathTokens = fileName.Split('/');
                string[] hms;
                int avgHour, avgMinute, avgSecond, totalSeconds = 0; // hours, minutes, seconds
                int fastestTotalSeconds = 0, fastestSecondsSoFar = 0;    

                int averageSeconds = 0;
                int fastestSeconds = 1000000000;

                string averageTimeStamp;
                string fastestTimestamp;

                // If there are completed games for this play field, load the list of times for it.
                if ((new FileInfo("../../../Completed/" + filePathTokens[0] + "/times.txt")).Exists)
                {
                    pathToGame = "../../../Completed/" + filePathTokens[0] + "/times.txt";

                    using (var completionTimesFile = new StreamReader(pathToGame))
                    {
                        while ((completionTime = completionTimesFile.ReadLine()) != null)
                        {
                            // Add timestamp to list of string timestamps.
                            completionTimes.Add(completionTime);
                        }

                        // Loop through list of time strings and parse them.
                        foreach (string timestamp in completionTimes)
                        {
                            // Get timestamps and do math on them to get average and fastest times.
                            hms = timestamp.Split(':');

                            avgHour = Convert.ToInt32(hms[0]);
                            avgMinute = Convert.ToInt32(hms[1]);
                            avgSecond = Convert.ToInt32(hms[2]);

                            fastestSecondsSoFar += avgSecond;
                            fastestSecondsSoFar += avgMinute * 60;
                            fastestSecondsSoFar += avgHour * 3600;

                            totalSeconds += avgSecond;
                            totalSeconds += avgMinute * 60;
                            totalSeconds += avgHour * 3600;
                            
                            // Get fastest time total seconds.
                            if (fastestSecondsSoFar < fastestSeconds) {
                                fastestSeconds = fastestSecondsSoFar;
                                fastestTotalSeconds = fastestSecondsSoFar;
                            }

                            // Reset time to check for next one.
                            fastestSecondsSoFar = 0;
                        }
                    }
                }

                // Convert seconds count to a timestamp.
                averageSeconds = totalSeconds / completionTimes.Count();

                int fastestHours = fastestTotalSeconds / 3600;
                int averageHours = averageSeconds / 3600;

                fastestTotalSeconds -= fastestHours * 3600;
                averageSeconds -= averageHours * 3600;

                int fastestMinutes = fastestTotalSeconds / 60;
                int averageMinutes = averageSeconds / 60;

                fastestTotalSeconds -= fastestMinutes * 60;
                averageSeconds -= averageMinutes * 60;

                // Display time information in message box.
                fastestTimestamp = string.Format("{0:00}:{1:00}:{2:00}", fastestHours, fastestMinutes, fastestTotalSeconds);
                string fastestTimestampOutput = string.Format("{0, 10}", fastestTimestamp);

                averageTimeStamp = string.Format("{0:00}:{1:00}:{2:00}", averageHours, averageMinutes, averageSeconds);
                string averageTimeStampOutput = string.Format("{0, 10}", averageTimeStamp);
                
                richTextMessages.AppendText("\nFastest:  " + fastestTimestampOutput);
                richTextMessages.AppendText("\nAverage:  " + averageTimeStampOutput);
            }
        }

        /*  
        *  Method:     setLabelColors
        *  
        *  Purpose:    Changes sum labels to different colors according to state of game vs solution.
        * 
        *  Arguments:  none
        *              
        *  Return:     void
        */
        private void setLabelColors() {
            for (int i = 0; i < numColumns; i++) {
                // Reset the label value.
                summationX[i] = 0;
                summationY[i] = 0;
                summationLeftDiag = 0;
                summationRightDiag = 0;

                // Add up column values.
                for (int r = 0; r < numColumns; r++) {
                    for (int c = 0; c < numColumns; c++) {
                        if (c == i) {
                            summationX[i] += gameMatrix[r, c];
                        }
                    }
                }

                sumsX[i].Text = summationX[i].ToString();

                // Add up row values.
                for (int c = 0; c < numColumns; c++) {
                    for (int r = 0; r < numColumns; r++) {
                        if (r == i) {
                            summationY[i] += gameMatrix[r, c];
                        }
                    }
                }

                sumsY[i].Text = summationY[i].ToString();

                // Add up left diagonal values.
                for (int c = 0; c < numColumns; c++) {
                    for (int r = 0; r < numColumns; r++) {
                        if (r == c) {
                            summationLeftDiag += gameMatrix[r, c];
                        }
                    }
                }

                sumsLeftDiag.Text = summationLeftDiag.ToString();

                // Add up right diagonal values.
                for (int r = 0, c = numColumns - 1; r < numColumns && c >= 0; r++, c--) {
                    summationRightDiag += gameMatrix[r, c];
                }

                sumsRightDiag.Text = summationRightDiag.ToString();

                // Check if sum of row or column has reached the solution sum. If they are under the solution value,
                // keep label white. If they are same as solution value, turn label green. If they overshoot solution
                // value or finish a row or column but sum is still wrong, turn label red.
                if (summationX[i] == summationXAnswer[i]) {
                    sumsX[i].ForeColor = Color.FromArgb(51, 204, 51);
                } else if (summationX[i] > summationXAnswer[i] || (columnFilled(gameMatrix, i) && summationX[i] != summationXAnswer[i])) {
                    sumsX[i].ForeColor = Color.Red;
                } else {
                    sumsX[i].ForeColor = Color.White;
                }

                if (summationY[i] == summationYAnswer[i]) {
                    sumsY[i].ForeColor = Color.FromArgb(51, 204, 51);
                } else if (summationY[i] > summationYAnswer[i] || (rowFilled(gameMatrix, i) && summationY[i] != summationYAnswer[i])) {
                    sumsY[i].ForeColor = Color.Red;
                } else {
                    sumsY[i].ForeColor = Color.White;
                }

                if (summationLeftDiag == summationLeftDiagAnswer) {
                    sumsLeftDiag.ForeColor = Color.FromArgb(51, 204, 51);
                } else if (summationLeftDiag > summationLeftDiagAnswer || (leftDiagFilled(gameMatrix) && (summationLeftDiag != summationLeftDiagAnswer))) {
                    sumsLeftDiag.ForeColor = Color.Red;
                } else {
                    sumsLeftDiag.ForeColor = Color.White;
                }

                if (summationRightDiag == summationRightDiagAnswer) {
                    sumsRightDiag.ForeColor = Color.FromArgb(51, 204, 51);
                } else if (summationRightDiag > summationRightDiagAnswer || (rightDiagFilled(gameMatrix) && (summationRightDiag != summationRightDiagAnswer))) {
                    sumsRightDiag.ForeColor = Color.Red;
                } else {
                    sumsRightDiag.ForeColor = Color.White;
                }
            }
        }

        /*  
        *  Method:     rowFilled
        *  
        *  Purpose:    Checks if a row has been filled.
        * 
        *  Arguments:  int[,]       Game matrix.
        *              int          matrix row.
        *              
        *  Return:     bool
        */
        private bool rowFilled(int[,] matrix, int row) {
            // If there are any unfilled cells in the row, return false.
            for (int c = 0; c < numColumns; c++) {
                if (gameMatrix[row, c] == 0) {
                    return false;
                }
            }

            return true;
        }

        /*  
        *  Method:     columnFilled
        *  
        *  Purpose:    Checks if a column has been filled.
        * 
        *  Arguments:  int[,]       Game matrix.
        *              int          matrix column.
        *              
        *  Return:     bool
        */
        private bool columnFilled(int[,] matrix, int column) {
            // If there are any unfilled cells in the row, return false.
            for (int r = 0; r < numColumns; r++) {
                if (gameMatrix[r, column] == 0) {
                    return false;
                }
            }

            return true;
        }

        /*  
        *  Method:     leftDiagFilled
        *  
        *  Purpose:    Checks if left diagonal has been filled.
        * 
        *  Arguments:  int[,]       Game matrix.
        *              
        *  Return:     bool
        */
        private bool leftDiagFilled(int[,] matrix) {
            // If there are any unfilled cells in the row, return false.
            for (int r = 0; r < numColumns; r++) {
                for (int c = 0; c < numColumns; c++) {
                    if (r == c && gameMatrix[r, c] == 0) {
                        return false;
                    }
                }
            }

            return true;
        }

        /*  
        *  Method:     rightDiagFilled
        *  
        *  Purpose:    Checks if right diagonal has been filled.
        * 
        *  Arguments:  int[,]       Game matrix.
        *              
        *  Return:     bool
        */
        private bool rightDiagFilled(int[,] matrix) {
            // If there are any unfilled cells in the row, return false.
            for (int r = 0, c = numColumns - 1; r < numColumns && c >= 0; r++, c--) {
                if (gameMatrix[r, c] == 0) {
                    return false;
                }
            }

            return true;
        }

        /*  
        *  Method:     buttonSave_Click
        *  
        *  Purpose:    Handles when user clicks to save a game.
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        private void buttonSave_Click(object sender, EventArgs e) {
            int columnCounter = 0;

            string[] fileStuff = fileName.Split('/');

            // If save file doesn't exist, make it.
            if (!(new FileInfo("../../../Saves/" + fileStuff[0] + "/")).Exists) {
                (new FileInfo("../../../Saves/" + fileStuff[0] + "/")).Directory.Create();
            }
            
            // Write out save data.
            using (StreamWriter saveFile = new StreamWriter("../../../Saves/" + fileName)) {
                foreach (int value in initialMatrix){
                    if (columnCounter == numColumns) {
                        saveFile.Write("\n");
                        columnCounter = 0;
                    }

                    // Write file.
                    saveFile.Write(value);

                    columnCounter++;
                }

                saveFile.Write("\n\n");

                columnCounter = 0;

                // Save game data to file.
                foreach (int value in gameMatrix) {
                    if (columnCounter == numColumns) {
                        saveFile.Write("\n");
                        columnCounter = 0;
                    }

                    saveFile.Write(value);

                    columnCounter++;
                }

                saveFile.Write("\n\n");

                columnCounter = 0;

                // Save solution data to file.
                foreach (int value in solutionMatrix) {
                    if (columnCounter == numColumns) {
                        saveFile.Write("\n");
                        columnCounter = 0;
                    }

                    saveFile.Write(value);

                    columnCounter++;
                }

                // Save game's elapsed time timestamp.
                saveFile.Write("\n\n" + labelTimer.Text);
            }
        }

        /*  
        *  Method:     gameComplete
        *  
        *  Purpose:    Checks to see if player completed game.
        * 
        *  Arguments:  none
        *              
        *  Return:     bool
        */
        private bool gameComplete() {
            /*
             *  If all labels are green, game is done!
             * 
             */
            foreach (Label l in sumsX) {
                if (l.ForeColor != Color.FromArgb(51, 204, 51)) {
                    return false;  
                }
            }

            foreach (Label l in sumsY) {
                if (l.ForeColor != Color.FromArgb(51, 204, 51)) {
                    return false;
                }
            }

            if (sumsLeftDiag.ForeColor != Color.FromArgb(51, 204, 51) || sumsRightDiag.ForeColor != Color.FromArgb(51, 204, 51)) {
                return false;
            }

            return true;
        }

        /*  
        *  Method:     buttonHelp_Click
        *  
        *  Purpose:    Helps player by placing a correct value in an empty box.
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        private void buttonHelp_Click(object sender, EventArgs e) {
            bool foundUnfilledCell = false; // Whether a cell has been filled.
            Random rand;    // Random number for selecting game cell to fill.

            int row, col;
            int textBoxIndex;

            while (!foundUnfilledCell) {
                rand = new Random(); // Random number generator.

                // Choose random row and column.
                row = rand.Next(0, numColumns);
                col = rand.Next(0, numColumns);

                // Find random cell and see if it is blank.
                if (gameMatrix[row, col] == 0) {
                    // Update game model with the added solution value for that cell.
                    gameMatrix[row, col] = solutionMatrix[row, col];

                    // Find which text box to put the solution value.
                    textBoxIndex = row * numColumns + col;

                    // Set text box value.
                    textBoxes[textBoxIndex].Text = solutionMatrix[row, col].ToString();

                    // We did find a cell and deal with it, so flip this flag to true.
                    foundUnfilledCell = true;
                }
            }
        }
    }

   /*  
    *  Class:      GameCell
    *  
    *  Purpose:    A cell in the game. Player enters a value into it.
    * 
    */
    public class GameCell : TextBox {
        // Import OS library for hiding caret in textboxes.
        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        public GameCell() {
            this.BackColor = Color.White;
            this.GotFocus += textBox_Selected;
            this.Leave += textBox_Deselected;
            this.KeyPress += textBox_KeyPress;
        }

        /*  
        *  Method:     textBox_Selected
        *  
        *  Purpose:    Handles when text box is selected.
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        void textBox_Selected(object sender, EventArgs e) {
            Control control = sender as Control;
            control.BackColor = Color.FromArgb(120, 200, 255);

            HideCaret(this.Handle);
        }

        /*  
        *  Method:     textBox_Deselected
        *  
        *  Purpose:    Handles when user leaves a textbox.
        * 
        *  Arguments:  object          UI component sending event.
        *              EventArgs       The event.
        *              
        *  Return:     void
        */
        void textBox_Deselected(object sender, EventArgs e) {
            Control control = sender as Control;

            // Reset color
            control.BackColor = Color.FromArgb(255, 255, 255);
        }

        /*  
        *  Method:     textBox_KeyPress
        *  
        *  Purpose:    Handles when a user types into a textbox.
        * 
        *  Arguments:  object               UI component sending event.
        *              KeyPressEventArgs    The event.
        *              
        *  Return:     void
        */
        private void textBox_KeyPress(object sender, KeyPressEventArgs e) {
            // Allow textBox to only accept numbers 1-9.
            if (!(((Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back)) && (e.KeyChar != '0')))) {
                // If the key pressed was not a number within 1-9, then Handle it (meaning DON'T LET PROCESSING GO FURTHER).
                e.Handled = true;
            }
        }     
    }

    /*  
        *  Class:      rowColTag
        *  
        *  Purpose:    Contains some data for a textbox (the row and column mapping to the game matrix).
        * 
        *  Return:     void
        */
    public class rowColTag {
        public int row; // Matrix row.
        public int col; // Matrix column.

        public rowColTag(int newRow, int newCol) {
            row = newRow;
            col = newCol;
        }
    }
}
