
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using StudentScoreManager.Models;

namespace StudentScoreManager.Forms
{
    /// <summary>
    /// 课堂课程卡片：一节课（一个节次）对应一张卡片。
    /// 自绘实现圆角 + 选中高亮 + 无课置灰占位，不依赖子控件，避免透明/布局兼容问题。
    /// </summary>
    public sealed class CourseCard : UserControl
    {
        // 当前卡片代表的课程；为 null 表示本节无课程（占位卡）。
        public CourseItem? Course { get; private set; }
        // 备注（来自 TeachingLogs 探测到的备注列；无则为空）。
        public string Remark { get; private set; } = "";
        // 节次标签（如"一、二"）。
        public string SectionLabel { get; private set; } = "";
        // 是否处于选中态。
        private bool _selected;
        public bool IsSelected
        {
            get => _selected;
            set { if (_selected != value) { _selected = value; Invalidate(); } }
        }

        // 配色
        private static readonly Color BaseBorder = Color.FromArgb(224, 224, 224);
        private static readonly Color BaseBack = Color.White;
        private static readonly Color SelBorder = Color.FromArgb(0, 120, 212);
        private static readonly Color SelBack = Color.FromArgb(233, 243, 253);
        private static readonly Color EmptyBack = Color.FromArgb(243, 243, 243);
        private static readonly Color EmptyFore = Color.FromArgb(165, 165, 165);
        private static readonly Color TitleFore = Color.FromArgb(32, 32, 32);
        private static readonly Color MetaFore = Color.FromArgb(96, 96, 96);

        public CourseCard()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
            Size = new Size(280, 150);
            Margin = new Padding(6, 4, 6, 4);
            Padding = new Padding(0);
            Cursor = Cursors.Hand;
            BackColor = Color.Transparent;
        }

        /// <summary>有课：填入课程信息。</summary>
        public void SetData(CourseItem item, string? sectionLabel, string? remark)
        {
            Course = item;
            SectionLabel = sectionLabel ?? "";
            Remark = remark ?? "";
            Enabled = true;
            Invalidate();
        }

        /// <summary>无课：占位卡，置灰禁用。</summary>
        public void SetEmpty(string sectionLabel)
        {
            Course = null;
            SectionLabel = sectionLabel ?? "";
            Remark = "";
            Enabled = false;
            Invalidate();
        }

        /// <summary>由父容器在宽度变化时调用，使卡片始终铺满可用宽度。</summary>
        public void SetCardWidth(int width)
        {
            if (width < 80) width = 80;
            if (Width != width) Width = width;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int w = Width, h = Height;
            var rect = new Rectangle(0, 0, w - 1, h - 1);
            int radius = 10;

            Color back, border, titleFore, metaFore;
            if (!Enabled) { back = EmptyBack; border = BaseBorder; titleFore = EmptyFore; metaFore = EmptyFore; }
            else if (_selected) { back = SelBack; border = SelBorder; titleFore = TitleFore; metaFore = MetaFore; }
            else { back = BaseBack; border = BaseBorder; titleFore = TitleFore; metaFore = MetaFore; }

            using (var path = Rounded(rect, radius))
            using (var bb = new SolidBrush(back))
            using (var pen = new Pen(border, _selected ? 2f : 1f))
            {
                g.FillRectangle(bb, rect);
                g.DrawPath(pen, path);
            }

            using (var fSection = new Font(Font.FontFamily, 8.5f, FontStyle.Bold))
            using (var fTitle = new Font(Font.FontFamily, 10.5f, FontStyle.Bold))
            using (var fMeta = new Font(Font.FontFamily, 8.5f, FontStyle.Regular))
            using (var brTitle = new SolidBrush(titleFore))
            using (var brMeta = new SolidBrush(metaFore))
            using (var brSection = new SolidBrush(_selected ? SelBorder : MetaFore))
            {
                var sfEll = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap, LineAlignment = StringAlignment.Center };

                if (!Enabled)
                {
                    DrawCenteredText(g, $"节次 {SectionLabel}", fSection, EmptyFore, new Rectangle(12, 12, w - 24, 22));
                    DrawCenteredText(g, "本节无课程", fTitle, EmptyFore, new Rectangle(12, h / 2 - 14, w - 24, 28));
                    return;
                }

                int x = 14, y = 10;
                // 第一行：节次角标 + 课次编号
                string secText = "节次  " + SectionLabel;
                var secSize = g.MeasureString(secText, fSection);
                g.DrawString(secText, fSection, brSection, new Point(x, y));
                if (Course != null && !string.IsNullOrEmpty(Course.CourseCode))
                {
                    g.DrawString(Course.CourseCode, fSection, brMeta,
                        new RectangleF(x + secSize.Width + 12, y, w - x - (int)secSize.Width - 12 - 14, secSize.Height), sfEll);
                }
                y += (int)secSize.Height + 6;

                // 第二行：课程名称（大字）
                g.DrawString(Safe(Course?.CourseName), fTitle, brTitle,
                    new RectangleF(x, y, w - 2 * x, fTitle.GetHeight(g)), sfEll);
                y += (int)fTitle.GetHeight(g) + 6;

                // 明细行
                var lines = new[]
                {
                    "班级：" + Safe(Course?.ClassName),
                    "教室：" + Safe(Course?.Classroom),
                    "内容：" + Safe(Course?.Content),
                    "备注：" + (string.IsNullOrEmpty(Remark) ? "无" : Remark),
                };
                foreach (var ln in lines)
                {
                    g.DrawString(ln, fMeta, brMeta, new RectangleF(x, y, w - 2 * x, fMeta.GetHeight(g) + 1), sfEll);
                    y += (int)fMeta.GetHeight(g) + 3;
                }
            }
        }

        private static string Safe(string? s) => string.IsNullOrEmpty(s) ? "—" : s;

        private static void DrawCenteredText(Graphics g, string text, Font font, Color color, Rectangle rect)
        {
            using var b = new SolidBrush(color);
            var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(text, font, b, rect, sf);
        }

        private static GraphicsPath Rounded(Rectangle r, int radius)
        {
            int d = radius * 2;
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }
    }
}