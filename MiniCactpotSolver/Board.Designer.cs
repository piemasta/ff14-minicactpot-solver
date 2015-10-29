using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using MiniCactpotSolver.Properties;

namespace MiniCactpotSolver
{
    partial class Board
    {
        #region Bitmap declarations
        private Bitmap label_cactpot = Resources.label_cactpot;
        private Bitmap light_red = Resources.light_red;
        private Bitmap light_green = Resources.light_green;

        private Bitmap blank = Resources.blank_default;
        private Bitmap prediction = Resources.blank_predicted;
        private Bitmap background = Resources.board_background;

        private Bitmap close_default = Resources.button_close_default;
        private Bitmap close_hover = Resources.button_close_hover;
        private Bitmap close_depressed = Resources.button_close_depressed;
        
        private Bitmap[] numbers = { Resources.number_1_default, Resources.number_2_default, Resources.number_3_default, Resources.number_4_default,
                                     Resources.number_5_default, Resources.number_6_default, Resources.number_7_default, Resources.number_8_default,
                                     Resources.number_9_default };

        private Bitmap[] arrows_default = { Resources.arrow_right_default, Resources.arrow_right_default, Resources.arrow_right_default,
                                            Resources.arrow_down_default,  Resources.arrow_down_default,  Resources.arrow_down_default, 
                                            Resources.arrow_downright_default, Resources.arrow_downleft_default };


        private Bitmap[] arrows_active = { Resources.arrow_right_active, Resources.arrow_right_active, Resources.arrow_right_active, 
                                           Resources.arrow_down_active, Resources.arrow_down_active,  Resources.arrow_down_active, 
                                           Resources.arrow_downright_active, Resources.arrow_downleft_active };

        private Bitmap[] arrows_selected = { Resources.arrow_right_selected, Resources.arrow_right_selected, Resources.arrow_right_selected, 
                                             Resources.arrow_down_selected, Resources.arrow_down_selected,  Resources.arrow_down_selected, 
                                             Resources.arrow_downright_selected, Resources.arrow_downleft_selected };
        #endregion

        private PictureBox[] tiles;
        private PictureBox[] arrows;
        private PictureBox button_close;
        private PictureBox light_cactpot_status;
        
        #region Initialize Component
        private void InitializeComponent()
        {
            tiles = new PictureBox[9];
            arrows = new PictureBox[8];

            light_cactpot_status = new PictureBox();
            light_cactpot_status.Margin = new Padding(0);
            light_cactpot_status.Size = new Size(20, 20);
            light_cactpot_status.BackColor = Color.Transparent;
            light_cactpot_status.SizeMode = PictureBoxSizeMode.StretchImage;
            light_cactpot_status.Image = light_green;
            light_cactpot_status.TabStop = false;
            light_cactpot_status.Tag = -1;
            light_cactpot_status.Location = new Point(475, 277);
            this.Controls.Add(light_cactpot_status);
            
            this.SuspendLayout();
            #region Initialization of board buttons
            button_close = new PictureBox();
            button_close.Margin = new Padding(0);
            button_close.Size = new Size(114, 21);
            button_close.BackColor = Color.Transparent;
            button_close.Image = Resources.button_close_default;
            button_close.Location = new Point(84, 277);
            button_close.TabStop = false;
            button_close.Tag = -1;
            button_close.MouseClick += new MouseEventHandler(Button_Close_MouseClick);
            button_close.MouseDown += new MouseEventHandler(Button_Close_MouseDown);
            button_close.MouseUp += new MouseEventHandler(Button_Close_MouseUp);
            button_close.MouseEnter += new EventHandler(Button_Close_MouseEnter);
            button_close.MouseLeave += new EventHandler(Button_Close_MouseLeave);
            this.Controls.Add(button_close);
            #endregion
            #region Initialization of board tiles (@tiles[0-8])
            for (int x = 0; x < 9; x++)
            {
                tiles[x] = new PictureBox();
                tiles[x].Margin = new Padding(0);
                tiles[x].Size = new Size(48, 48);
                tiles[x].BackColor = Color.Transparent;
                tiles[x].TabStop = false;
                tiles[x].Name = ((char)x) + "";
                tiles[x].Tag = -1;
                tiles[x].Image = blank;
                this.Controls.Add(tiles[x]);
            }
            tiles[0].Location = new Point(62, 77);
            tiles[1].Location = new Point(116, 77);
            tiles[2].Location = new Point(170, 77);
            tiles[3].Location = new Point(62, 131);
            tiles[4].Location = new Point(116, 131);
            tiles[5].Location = new Point(170, 131);
            tiles[6].Location = new Point(62, 185);
            tiles[7].Location = new Point(116, 185);
            tiles[8].Location = new Point(170, 185);            
            #endregion
            #region Initialization of arrow buttons (@arrows[0-7])          
            for (int x = 0; x < 8; x++)
            {
                arrows[x] = new PictureBox();
                arrows[x].Margin = new Padding(0);
                arrows[x].Size = new Size(28, 28);
                arrows[x].BackColor = Color.Transparent;                
                arrows[x].TabStop = false;
                arrows[x].Name = ((char)x)+ "";
                arrows[x].Tag = -1;
                arrows[x].Image = arrows_default[x];
                arrows[x].MouseEnter += new EventHandler(Arrow_MouseEnter);
                arrows[x].MouseLeave += new EventHandler(Arrow_MouseLeave);
                this.Controls.Add(arrows[x]);
            }
            arrows[0].Location = new Point(30, 86);
            arrows[1].Location = new Point(30, 140);
            arrows[2].Location = new Point(30, 194);
            arrows[3].Location = new Point(72, 46);
            arrows[4].Location = new Point(126, 46);
            arrows[5].Location = new Point(180, 46);
            arrows[6].Location = new Point(30, 46);
            arrows[7].Location = new Point(220, 46);
            #endregion 
            #region Initialization of board
            this.StartPosition = FormStartPosition.CenterScreen;
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.FormBorderStyle = FormBorderStyle.None;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.Icon = Resources.icon;
            this.ClientSize = new Size(540, 318);
            this.MaximumSize = new Size(540, 318);
            this.MinimumSize = new Size(540, 318);
            this.BackColor = Color.Cyan;
            this.TransparencyKey = this.BackColor;            
            this.MaximizeBox = false;
            this.ShowIcon = false;
            this.ShowInTaskbar = true;
            this.TopMost = true;
            this.Name = "Board";
            this.Text = "MiniCactpot";
            this.Tag = -1;
            this.FormClosing += new FormClosingEventHandler(this.Board_FormClosing);
            this.MouseDown += new MouseEventHandler(this.Board_MouseDown);
            this.Paint += new PaintEventHandler(this.Board_Paint);
            #endregion
            this.ResumeLayout(false);            
        }
        #endregion
    }
}

