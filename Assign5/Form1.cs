/*
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
using System.Runtime.InteropServices;

namespace Assign5 {
    public partial class Form1 : Form {
        string gameData;
        int numColumns;

        Dictionary<string, string> difficultyOptions = new Dictionary<string, string>();    // Holds game difficulty options for Difficulty dropdown menu.
        string fileName;
        string pathToGame;

        bool savedGame = false;

        GameCell[] textBoxes;

        Label[] sumsX;
        Label[] sumsY;
        Label[] sumsXAnswer;
        Label[] sumsYAnswer;

        int[] summationX;
        int[] summationY;
        int[] summationXAnswer;
        int[] summationYAnswer;

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

        private void button1_Start(object sender, EventArgs e) {
            int row = 0;

            buttonSave.Enabled = true;
            savedGame = false;

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
                }
            }

            fileName = ((KeyValuePair<string, string>)comboBoxGame.SelectedItem).Key;
            
            // If there is a saved game for this difficulty and field, then use that one.
            if ((new FileInfo("../../../Saves/" + fileName)).Exists) {
                pathToGame = "../../../Saves/" + fileName;
                savedGame = true;
            } else {
                pathToGame = "../../../Resources/" + fileName;
            }

            using (var gameFile = new StreamReader(pathToGame)) {
                Console.WriteLine("LINE: " + gameFile);
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

                    foreach (int val in initialMatrix) {
                        Console.WriteLine(val);
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

                row = 0;

                getAnswers();
                drawGame();
            }
        }

        private void getAnswers() {
            summationXAnswer = new int[numColumns];
            summationYAnswer = new int[numColumns];

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
        }

        private void drawGame() {
            textBoxes = new GameCell[numColumns * numColumns];

            sumsX = new Label[numColumns];
            sumsY = new Label[numColumns];

            summationX = new int[numColumns];
            summationY = new int[numColumns];

            rowColTag rowCol;

            int x = 300;
            int y = 60;

            int row = 0;
            int col = 0;
            
            for (int i = 0; i < numColumns * numColumns; i++) {
                // If we drew out the amount of columns we need, drop to new row.
                if (i % numColumns == 0 && i > 0) {
                    y += 385 / numColumns - 1;

                    x = 300;
                    row++;
                    col = 0;
                }

                textBoxes[i] = new GameCell();

                // Set properties of the textBox.
                textBoxes[i].Multiline = true;

                // Set cell font size according to cell size.
                if (gameMatrix.Length == 9) {
                    textBoxes[i].Font = new Font(textBoxes[i].Font.FontFamily, 60);
                } else if (gameMatrix.Length == 25) {
                    textBoxes[i].Font = new Font(textBoxes[i].Font.FontFamily, 40);
                } else {
                    textBoxes[i].Font = new Font(textBoxes[i].Font.FontFamily, 30);
                }

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
                x += 385 / numColumns - 1;

                // Go to next column.
                col++;
            }

            drawSolutionLabels();

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
        }

        private void drawSolutionLabels() {
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
        }

        private void radioButtonEasy_CheckedChanged(object sender, EventArgs e) {
            difficultyOptions.Clear();
            comboBoxGame.Enabled = true;

            difficultyOptions.Add("easy/e1.txt", "Easy 1");
            difficultyOptions.Add("easy/e2.txt", "Easy 2");
            difficultyOptions.Add("easy/e3.txt", "Easy 3");

            // Set the dropdown menu to use the data source.
            comboBoxGame.DataSource = new BindingSource(difficultyOptions, null);
            comboBoxGame.DisplayMember = "Value";
            comboBoxGame.ValueMember = "Key";
        }

        private void radioButtonMedium_CheckedChanged(object sender, EventArgs e) {
            difficultyOptions.Clear();
            comboBoxGame.Enabled = true;
            
            difficultyOptions.Add("medium/m1.txt", "Medium 1");
            difficultyOptions.Add("medium/m2.txt", "Medium 2");
            difficultyOptions.Add("medium/m3.txt", "Medium 3");

            // Set the dropdown menu to use the data source.
            comboBoxGame.DataSource = new BindingSource(difficultyOptions, null);
            comboBoxGame.DisplayMember = "Value";
            comboBoxGame.ValueMember = "Key";
        }

        private void radioButtonHard_CheckedChanged(object sender, EventArgs e) {
            difficultyOptions.Clear();
            comboBoxGame.Enabled = true;
            
            difficultyOptions.Add("hard/h1.txt", "Hard 1");
            difficultyOptions.Add("hard/h2.txt", "Hard 2");
            difficultyOptions.Add("hard/h3.txt", "Hard 3");

            // Set the dropdown menu to use the data source.
            comboBoxGame.DataSource = new BindingSource(difficultyOptions, null);
            comboBoxGame.DisplayMember = "Value";
            comboBoxGame.ValueMember = "Key";
        }

        public void HideCaret(TextBox textbox) {
            HideCaret(textbox.Handle);
        }

        public void divertFocus(object sender, EventArgs e) {
            comboBoxGame.Focus();
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
            GameCell cell = (GameCell)sender;
            rowColTag rowCol = (rowColTag)cell.Tag;

            char[] textBoxChars = (cell.Text).ToCharArray();

            // If cell is blank, then set its value to 0, else update gameMatrix with cell's new value.
            if (cell.Text.Length == 0) {
                gameMatrix[rowCol.row, rowCol.col] = 0;
            } else {
                // Update gameMatrix at the specified element.
                gameMatrix[rowCol.row, rowCol.col] = Convert.ToInt32(cell.Text);
            }

            for (int i = 0; i < numColumns; i++) {
                // Reset the label value.
                summationX[i] = 0;
                summationY[i] = 0;

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
            }
        }

        private bool rowFilled(int[,] matrix, int row) {
            // If there are any unfilled cells in the row, return false.
            for (int c = 0; c < numColumns; c++) {
                if (gameMatrix[row, c] == 0) {
                    return false;
                }
            }

            return true;
        }

        private bool columnFilled(int[,] matrix, int column) {
            // If there are any unfilled cells in the row, return false.
            for (int r = 0; r < numColumns; r++) {
                if (gameMatrix[r, column] == 0) {
                    return false;
                }
            }

            return true;
        }

        private void buttonSave_Click(object sender, EventArgs e) {
            int columnCounter = 0;

            string[] fileStuff = fileName.Split('/');

            Console.WriteLine(fileStuff[0]);

            if (!(new FileInfo("../../../Saves/" + fileStuff[0] + "/")).Exists) {
                (new FileInfo("../../../Saves/" + fileStuff[0] + "/")).Directory.Create();
            }
            
            using (StreamWriter saveFile = new StreamWriter("../../../Saves/" + fileName)) {
                foreach (int value in initialMatrix){
                    if (columnCounter == numColumns) {
                        saveFile.Write("\n");
                        columnCounter = 0;
                    }

                    saveFile.Write(value);

                    columnCounter++;
                }

                saveFile.Write("\n\n");

                columnCounter = 0;

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

                foreach (int value in solutionMatrix) {
                    if (columnCounter == numColumns) {
                        saveFile.Write("\n");
                        columnCounter = 0;
                    }

                    saveFile.Write(value);

                    columnCounter++;
                }
            }
        }
    }

    public class GameCell : TextBox {
        [DllImport("user32.dll")]
        static extern bool HideCaret(IntPtr hWnd);

        public GameCell() {
            this.BackColor = Color.White;
            this.GotFocus += textBox_Selected;
            this.Leave += textBox_Deselected;
            this.KeyPress += textBox_KeyPress;
        }

        private void TextBoxGotFocus(object sender, EventArgs args) {
            HideCaret(this.Handle);
        }

        void textBox_Selected(object sender, EventArgs e) {
            Control control = sender as Control;
            control.BackColor = Color.FromArgb(120, 200, 255);

            HideCaret(this.Handle);
        }

        void textBox_Deselected(object sender, EventArgs e) {
            Control control = sender as Control;

            // Reset color
            control.BackColor = Color.FromArgb(255, 255, 255);
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e) {
            // Allow textBox to only accept numbers 1-9.
            if (!(((Char.IsDigit(e.KeyChar) || (e.KeyChar == (char)Keys.Back)) && (e.KeyChar != '0')))) {
                // If the key pressed was not a number within 1-9, then Handle it (meaning DON'T LET PROCESSING GO FURTHER).
                e.Handled = true;
            }
        }     
    }

    public class rowColTag {
        public int row;
        public int col;

        public rowColTag(int newRow, int newCol) {
            row = newRow;
            col = newCol;
        }
    }
}
