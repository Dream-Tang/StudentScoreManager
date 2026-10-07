
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using StudentScoreManager.Models;

namespace StudentScoreManager.Forms
{
    /// <summary>
    /// 左侧课程卡片（自绘圆角面板，无第三方依赖）。
    /// 一张卡片 = 一个节次；无课时置灰占位（Enabled=false、文字变灰、显示【本节无课程】）。
    /// 性能要点：
    ///   1) 开启双缓冲（OptimizedDoubleBuffer + AllPaintingInWmPaint + UserPaint + ResizeRedraw），
    ///      消除圆角/文字重绘时的闪烁；
    ///   2) 卡片复用：切换内容用 SetCard 就地更新，不重建控件；
    ///   3) Selected 变化自动 Invalidate，旧卡片高亮也能即时消失。
    /// 只用 GDI+（Graphics.DrawString / MeasureString），不用 TextRenderer，避免其在不同 .NET
    /// 版本间的重载差异导致编译不一致。
    /// </summary>
    public sealed class CourseCard : Panel
    {
        private const int CardHeight = 172;
        private const int Radius = 10;
        private const int Pad = 14;

        private static readonly Font FontSection = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontTitle = new Font("Microsoft YaHei UI", 11.5F, FontStyle.Bold, GraphicsUnit.Point);
        private static readonly Font FontBody = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly Font FontState = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
        private static readonly ToolTip Tip = new ToolTip { AutoPopDelay = 8000, InitialDelay = 400, ReshowDelay = 200 };

        // 主题色
        private static readonly Color Accent = Color.FromArgb(0, 120, 215);
        private static readonly Color CardBack = Color.White;
        private static readonly Color CardBackSelected = Color.FromArgb(232, 243, 253);
        private static readonly Color CardBackEmpty = Color.FromArgb(246, 247, 249);
        private static readonly Color CardBorder = Color.FromArgb(224, 227, 231);
        private static readonly Color CardBorderHover = Color.FromArgb(160, 200, 235);
        private static readonly Color TextMain = Color.FromArgb(40, 44, 50);
        private static readonly Color TextMuted = Color.FromArgb(120, 126, 135);
        private static readonly Color TextDisabled = Color.FromArgb(175, 178, 182);

        private bool _selected;
        private bool _hover;

        /// <summary>当前卡片代表的课程；为 null 表示本节无课程（占位卡）。</summary>
        public CourseItem? Course { get; private set; }
        /// <summary>节次标签（如"一、二"）。</summary>
        public string Section { get; private set; } = "";
        /// <summary>备注（仅用于卡片展示，不进数据库）。</summary>
        public string Remark { get; private set; } = "";

        /// <summary>是否处于选中态；设置后自动重绘（旧卡取消高亮也会即时刷新）。</summary>
        public bool Selected
        {
            get => _selected;
            set { if (_selected != value) { _selected = value; Invalidate(); } }
        }

        public CourseCard()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Margin = new Padding(0, 0, 0, 10);
            Size = new Size(300, CardHeight);
            Height = CardHeight;           // 高度固定，只让宽度跟随左侧面板
            BackColor = Color.Transparent;
            TabStop = false;
        }

        /// <summary>就地更新卡片内容并复用（切换日期时调用，避免重建控件）。</summary>
        public void SetCard(CourseItem? item, string? section, string? remark)
        {
            Course = item;
            Section = section ?? "";
            Remark = remark ?? "";

            bool has = Has(item);
            Enabled = has;
            Cursor = has ? Cursors.Hand : Cursors.Default;

            string tip = has ? BuildTipText(item!) : "";
            if (!string.IsNullOrEmpty(tip)) Tip.SetToolTip(this, tip);
            else Tip.SetToolTip(this, "");

            Invalidate();
        }

        /// <summary>有效课程判定：LogID 存在且非 0。</summary>
        private static bool Has(CourseItem? it)
            => it != null && it.LogID.HasValue && it.LogID.Value != 0;

        private bool HasContent => Has(Course);

        private string BuildTipText(CourseItem it)
            => $"课程：{it.CourseName}\r\n班级：{it.ClassName}\r\n教室：{it.Classroom}\r\n" +
               $"教学内容：{it.Content}\r\n备注：{Remark}";

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; Invalidate(); base.OnMouseLeave(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var it = Course;
            bool has = HasContent;

            // 卡片底 + 描边（选中=强调蓝 2px，悬停=浅蓝，常态=浅灰）
            using (var path = Round(new RectangleF(1, 1, Width - 3, Height - 3), Radius))
            using (var back = new SolidBrush(!has ? CardBackEmpty : (Selected ? CardBackSelected : CardBack)))
                g.FillPath(back, path);

            Color borderColor = !has ? Color.FromArgb(232, 233, 235)
                              : Selected ? Accent
                              : _hover ? CardBorderHover : CardBorder;
            float inset = Selected ? 2f : 1f;
            using (var pen = new Pen(borderColor, Selected ? 2f : 1f))
            using (var path = Round(new RectangleF(1, 1, Width - 1 - 2 * inset, Height - 1 - 2 * inset), Radius))
                g.DrawPath(pen, path);

            // 顶部：节次徽标（左） + 课程代码（右）
            int top = 12;
            string section = $"第 {Section} 节";
            SizeF secSize = g.MeasureString(section, FontSection);
            var badge = new RectangleF(Pad, top, secSize.Width + 14, 22);
            Color badgeBack = has ? Color.FromArgb(226, 240, 253) : Color.FromArgb(238, 239, 241);
            Color badgeFore = has ? Accent : TextDisabled;
            using (var bp = Round(badge, 6))
            using (var bb = new SolidBrush(badgeBack)) g.FillPath(bb, bp);
            DrawText(g, section, FontSection, badge, badgeFore, StringAlignment.Near);

            string code = it?.CourseCode ?? "";
            if (!string.IsNullOrEmpty(code))
            {
                SizeF cs = g.MeasureString(code, FontState);
                var cr = new RectangleF(Width - Pad - cs.Width, top + 3, cs.Width + 2, cs.Height);
                DrawText(g, code, FontState, cr, has ? TextMuted : TextDisabled, StringAlignment.Far);
            }

            // 课程名称 / 无课占位标题
            int y = top + 32;
            string title = has ? (it!.CourseName ?? "") : "本节无课程";
            DrawText(g, title, FontTitle,
                new RectangleF(Pad, y, Width - Pad * 2, 26),
                has ? TextMain : TextDisabled, StringAlignment.Near);

            // 明细：班级 / 教室 / 内容 / 备注
            y += 30;
            int lh = 22;
            DrawRow(g, ref y, lh, "班级", has ? it!.ClassName : "");
            DrawRow(g, ref y, lh, "教室", has ? it!.Classroom : "");
            DrawRow(g, ref y, lh, "内容", has ? it!.Content : "");
            DrawRow(g, ref y, lh, "备注", has ? Remark : "");

            if (!has)
                DrawText(g, "本节无课程", FontState,
                    new RectangleF(Pad, Height - 28, Width - Pad * 2, 18),
                    TextDisabled, StringAlignment.Near);
        }

        private void DrawRow(Graphics g, ref int y, int lh, string key, string val)
        {
            DrawText(g, key, FontBody, new RectangleF(Pad, y, 48, lh), TextMuted, StringAlignment.Near);
            string text = string.IsNullOrEmpty(val) ? "-" : val;
            var rect = new RectangleF(Pad + 48, y, Width - Pad - 48 - Pad, lh);
            DrawText(g, text, FontBody, rect, HasContent ? TextMain : TextDisabled, StringAlignment.Near);
            y += lh;
        }

        /// <summary>统一绘制：垂直居中 + 超宽省略号（& 不解释为助记键）。</summary>
        private static void DrawText(Graphics g, string text, Font font, RectangleF rect, Color color, StringAlignment align)
        {
            if (string.IsNullOrEmpty(text)) return;
            using var sf = new StringFormat();
            sf.Trimming = StringTrimming.EllipsisCharacter;
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = align;
            using var brush = new SolidBrush(color);
            g.DrawString(text, font, brush, rect, sf);
        }

        private static GraphicsPath Round(RectangleF r, float radius)
        {
            float d = radius * 2f;
            var p = new GraphicsPath();
            if (d <= 0f || d > r.Width || d > r.Height)
            {
                p.AddRectangle(r);
                p.CloseFigure();
                return p;
            }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}