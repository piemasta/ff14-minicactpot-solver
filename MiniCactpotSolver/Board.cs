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
using System.Drawing.Imaging;
using System.Security.Permissions;
using MiniCactpotSolver.Properties;
using System.Drawing.Drawing2D;

#region BUGS
/*
 * */
#endregion

#region TODO
/* 
 * 
 * *** Add tooltips over each payout listing showing which number combinations result in that payout. A sort of 'At A Glance' feature for
 * which lines might have payed out larger.
 * 
 * */
#endregion


namespace MiniCactpotSolver
{
    public partial class Board : Form
    {
        #region Definitions
        private int mouseOver = 0;
        private int tilesRegistered = 0;
        private bool user_revealed_tile = false;
        private bool user_covered_tile = false;
        private bool reactivate_arrow = false;
        private bool block_new_tiles = false;
        private bool show_odds = false;
        private bool[] tile_flags = new bool[9];
        private event EventHandler UserAction;

        private int[] prediction_indices;
        private int[] payout_chances;

        private Predictor p;

        #region Imported Functions
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        private static extern bool ReleaseCapture();
        #endregion
        #endregion

        public Board()
        {
            InitializeComponent();
            AddAllEventHandlers();
            p = new Predictor(ref tiles);
        }

        #region Game Modes
        private void PredictionMode()
        {
            if ((user_revealed_tile || user_covered_tile) && (tilesRegistered != 0))
            {
                prediction_indices = p.GetPrediction();
                for (int x = 0; x < prediction_indices.Length; x++)
                {
                    if (tilesRegistered == 4)
                        arrows[prediction_indices[x]].Image = arrows_active[prediction_indices[x]];

                    else
                        tiles[prediction_indices[x]].Image = prediction;
                }
            }

            if (p.cactpot_possible)
                light_cactpot_status.Image = light_green;
            else
                light_cactpot_status.Image = light_red;

            if (tilesRegistered < 4)
                block_new_tiles = false;
            else if (tilesRegistered == 4)
                block_new_tiles = true;

            user_covered_tile = false;
            user_revealed_tile = false;
        }
        #endregion

        #region Board Management Functions
        private void ClearArrowModifiers()
        {
            for (int x = 0; x < 8; x++)
            {
                if (arrows[x].Image != arrows_default[x])
                    arrows[x].Image = arrows_default[x];
            }
        }
        private void ClearPredictionOverlays()
        {
            for (int x = 0; x < 9; x++)
            {
                if (tiles[x].Image == prediction)
                    tiles[x].Image = blank;
            }
        }
        private bool IsRegistered(int numberIndex)
        {
            if (!SanityCheck(numberIndex, 0, 8))
                return true;

            return tile_flags[numberIndex];
        }        
        private void Register(int numberIndex)
        {
            if (!SanityCheck(numberIndex, 0, 8))
                return;

            if (!IsRegistered(numberIndex))
            {
                tile_flags[numberIndex] = true;
                tilesRegistered++;
            }
        }
        private void ResetBoard()
        {
            block_new_tiles = true;
            SetUserActionable(false);
            for (int x = 0; x < 9; x++)
            {
                SetTileToBlank(x);
            }
            user_covered_tile = false;
            block_new_tiles = false;
            SetUserActionable(true);
        }
        private void SetTileToBlank(int tileIndex)
        {
            if (!SanityCheck(tileIndex, 0, 8))
                return;

            ClearPredictionOverlays();
            if (tiles[tileIndex].Image != blank)
            {
                if (!SanityCheck((int)tiles[tileIndex].Tag, 0, 8))
                    return;

                if(tilesRegistered == 4)
                    ClearArrowModifiers();
                Unregister((int)tiles[tileIndex].Tag);
                tiles[tileIndex].Image = blank;
                tiles[tileIndex].Tag = -1;
                if(UserAction != null)
                    user_covered_tile = true;
            }
            OnUserAction();
        }
        private void SetTileToNumber(int tileIndex, int numberIndex)
        {
            if (!SanityCheck(tileIndex, 0, 8) || !SanityCheck(numberIndex, 0, 8))
                return;

            if (IsRegistered(numberIndex))
                return;

            if (tiles[tileIndex].Image == numbers[numberIndex])
                return;

            ClearPredictionOverlays();
            if (tiles[tileIndex].Image == blank)
            {
                if (block_new_tiles)
                    return;
                tiles[tileIndex].Image = numbers[numberIndex];
                tiles[tileIndex].Tag = numberIndex;
                Register(numberIndex);
                user_revealed_tile = true;
            }
            else
            {
                if (!SanityCheck((int)tiles[tileIndex].Tag, 0, 8))
                    return;

                if (tilesRegistered == 4)
                    ClearArrowModifiers();
                Unregister((int)tiles[tileIndex].Tag);
                tiles[tileIndex].Image = numbers[numberIndex];
                tiles[tileIndex].Tag = numberIndex;
                Register(numberIndex);
                user_revealed_tile = true;
            }
            OnUserAction();
        }
        private void Unregister(int numberIndex)
        {
            if (!SanityCheck(numberIndex, 0, 8))
                return;

            if(IsRegistered(numberIndex))
            {
                tile_flags[numberIndex] = false;
                tilesRegistered--;
            }
        }
        #endregion

        #region Utility Functions
        private void AddAllEventHandlers()
        {
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Board_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Board_KeyPress);
            this.UserAction += Board_UserAction;
            for(int x = 0; x < 9; x++)
            {
                this.tiles[x].MouseEnter += new System.EventHandler(this.Tile_MouseEnter);
                this.tiles[x].MouseLeave += new System.EventHandler(this.Tile_MouseLeave);
            }
            for (int x = 0; x < 8; x++)
            {
                this.arrows[x].MouseEnter += new System.EventHandler(this.Tile_MouseEnter);
                this.arrows[x].MouseLeave += new System.EventHandler(this.Tile_MouseLeave);
            }
        }      
        private void InitializeEventHandlers()
        {
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Board_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Board_KeyPress);
            this.UserAction += Board_UserAction;
        }
        private void RemoveAllEventHandlers()
        {
            this.KeyDown -= new System.Windows.Forms.KeyEventHandler(this.Board_KeyDown);
            this.KeyPress -= new System.Windows.Forms.KeyPressEventHandler(this.Board_KeyPress);
            UserAction -= Board_UserAction;
            for (int x = 0; x < 9; x++)
            {
                this.tiles[x].MouseEnter -= new System.EventHandler(this.Tile_MouseEnter);
                this.tiles[x].MouseLeave -= new System.EventHandler(this.Tile_MouseLeave);
            }
            for (int x = 0; x < 8; x++)
            {
                this.arrows[x].MouseEnter -= new System.EventHandler(this.Tile_MouseEnter);
                this.arrows[x].MouseLeave -= new System.EventHandler(this.Tile_MouseLeave);
            }
        }
        private bool SanityCheck(int check, int low, int high)
        {
            if (low > high)
                return false;

            if (check < low || check > high)
                return false;
            else return true;
        }
        private void SetUserActionable(bool val)
        {
            if (val)
                this.UserAction += Board_UserAction;
            else
                this.UserAction -= Board_UserAction;
        }        
        #endregion

        #region Custom Event Definitions
        private void OnUserAction()
        {
            EventHandler handler = UserAction;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        #endregion

        #region Event Handlers
        private void Arrow_MouseEnter(object sender, EventArgs e)
        {
            if (tilesRegistered > 0)
            {
                int index = (int)(((Control)sender).Name[0]);
                payout_chances = p.GetPayoutChances(index);
                mouseOver = index;
                show_odds = true;
                this.Invalidate(new Rectangle(new Point(248, 86), new Size(36, 190)));
                this.Invalidate(new Rectangle(new Point(388, 86), new Size(36, 190)));
                if (arrows[index].Image == arrows_active[index]) reactivate_arrow = true;
                arrows[index].Image = arrows_selected[index];
            }
        }
        private void Arrow_MouseLeave(object sender, EventArgs e)
        {
            mouseOver = -1;
            show_odds = false;
            int index = (int)(((Control)sender).Name[0]);
            if (reactivate_arrow && (arrows[index].Image == arrows_selected[index]))
                arrows[index].Image = arrows_active[index];
            else if (arrows[index].Image == arrows_selected[index])
                arrows[index].Image = arrows_default[index];
            reactivate_arrow = false;
            this.Refresh();
        }

        private void Board_FormClosing(object sender, FormClosingEventArgs e)
        {
            //logger.Flush();
        }
        private void Board_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Application.Exit();

            if (e.KeyCode == Keys.Enter)
                ResetBoard();

            if (!SanityCheck(mouseOver, 0, 8))
            {
                e.Handled = true;
                return;
            }

            if (((e.KeyCode >= Keys.D0) && (e.KeyCode <= Keys.D9)) || ((e.KeyCode >= Keys.NumPad0) && (e.KeyCode <= Keys.NumPad9)))
                return;

            e.Handled = true;
        }
        private void Board_KeyPress(object sender, KeyPressEventArgs e)
        {
            int numberIndex = e.KeyChar - 48 - 1;
            if ((numberIndex + 1) == 0)
            {
                SetTileToBlank(mouseOver);
                return;
            }
            if (!SanityCheck(numberIndex, 0, 8) || !SanityCheck(mouseOver, 0, 8))
                return;

            SetTileToNumber(mouseOver, numberIndex);
        }
        private void Board_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Transparent, this.DisplayRectangle);

            ImageAttributes attributes = new ImageAttributes();
            attributes.SetColorKey(background.GetPixel(0, 0), background.GetPixel(0, 0));
            GraphicsUnit gu = GraphicsUnit.Pixel;
            e.Graphics.DrawImage((Image)background, Rectangle.Truncate(background.GetBounds(ref gu)), 0F, 0F,
                                 (float)background.Width, (float)background.Height, GraphicsUnit.Pixel, attributes);

            Font font = new Font("AXIS", 9, FontStyle.Regular, GraphicsUnit.Pixel);
            Brush brush = new SolidBrush(Color.FromArgb(255, 168, 168, 168));

            e.Graphics.DrawString("%", font, brush, new Point(248 + 8, 86 - 18));
            e.Graphics.DrawString("%", font, brush, new Point(388 + 8, 86 - 18));

            #region Draw line odds text
            if (show_odds)
            {   
                font = new Font("AXIS", 15, FontStyle.Regular, GraphicsUnit.Pixel);
                brush = new SolidBrush(Color.FromArgb(255, 230, 230, 230));

                Point L = new Point(248, 86);
                Point R = new Point(388, 86);

                for (int x = 6; x < 16; x++, L.Offset(0, 18))
                {
                    if(payout_chances[x] == 100)
                        e.Graphics.DrawString(payout_chances[x] + "", font, brush, L);
                    else if(payout_chances[x] > 9)
                        e.Graphics.DrawString(payout_chances[x] + "", font, brush, L.X + 4, L.Y);
                    else
                        e.Graphics.DrawString(payout_chances[x] + "", font, brush, L.X + 8, L.Y);
                }
                for (int x = 16; x < 25; x++, R.Offset(0, 18))
                {
                    if (payout_chances[x] == 100)
                        e.Graphics.DrawString(payout_chances[x] + "", font, brush, R);
                    else if (payout_chances[x] > 9)
                        e.Graphics.DrawString(payout_chances[x] + "", font, brush, R.X + 4, R.Y);
                    else
                        e.Graphics.DrawString(payout_chances[x] + "", font, brush, R.X + 8, R.Y);
                }
            }
            #endregion

            font = new Font("AXIS", 20, FontStyle.Regular, GraphicsUnit.Pixel);
            brush = new SolidBrush(Color.FromArgb(255, 240, 240, 240));
            e.Graphics.DrawImage((Image)label_cactpot, 330, 278, 133, 22);
        }
        private void Board_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
        private void Board_UserAction(object sender, EventArgs e)
        {
            p.Update();


            PredictionMode();
        }

        private void Button_Close_MouseEnter(object sender, EventArgs e)
        {
            ((PictureBox)sender).Image = close_hover;
        }
        private void Button_Close_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).Image = close_default;
        }
        private void Button_Close_MouseDown(object sender, MouseEventArgs e)
        {
            ((PictureBox)sender).Image = close_depressed;
        }
        private void Button_Close_MouseClick(object sender, MouseEventArgs e)
        {
            Application.Exit();
        }
        private void Button_Close_MouseUp(object sender, MouseEventArgs e)
        {
            if (((PictureBox)sender).Bounds.Contains(e.Location))
                ((PictureBox)sender).Image = close_hover;
            else
                ((PictureBox)sender).Image = close_default;
        }

        private void Tile_MouseEnter(object sender, EventArgs e)
        {
            mouseOver = (int)(((Control)sender).Name[0]);
        }
        private void Tile_MouseLeave(object sender, EventArgs e)
        {
            mouseOver = -1;
        }
        #endregion
    }
}
