
using Krypton.Toolkit;
// 文件路径: Program.cs
namespace StudentScoreManager
{
    internal static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // 开启系统 DPI 感知（推荐）
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            // 启用应用程序的视觉样式（WinForms 标准配置）
            Application.EnableVisualStyles();
            // 设置文本默认呈现方式
            Application.SetCompatibleTextRenderingDefault(false);

            // ==== 全局统一主题：在启动主窗体之前初始化一次即可 ====
            // 全局皮肤由 AppTheme 内部唯一的 KryptonManager 实例统一控制，
            // 只需初始化一次，Form1 / MainForm 及以后新增窗体都会随之统一换肤。
            AppTheme.Initialize();

            // 启动主窗体。
            Application.Run(new MainForm());
        }
    }

    /// <summary>
    /// 全局主题统一入口 + 运行期动态换肤：所有窗体共用这一处主题配置。
    /// 换肤只需调用 AppTheme.Apply(...)，所有窗体（含界面里的主题下拉框）会同步刷新。
    /// </summary>
    internal static class AppTheme
    {
        // 默认主题名（程序启动时应用；写字符串 + Enum.TryParse，规避不同 Krypton 版本枚举成员名差异导致的编译报错）。
        // 解析不到会自动保持 PaletteMode.Global，不抛异常。
        public const string ThemeName = "Microsoft365Silver";

        // 当前生效主题。
        public static PaletteMode Mode { get; private set; } = PaletteMode.Global;

        // 当前主题名（供下拉框回显）。
        public static string CurrentName => Mode.ToString();

        // 主题变化事件：任一窗体切换主题后触发，其它窗体可借此同步自身控件。
        public static event EventHandler? ThemeChanged;

        // ==== 全局唯一的 KryptonManager 实例 ====
        // 当前 Krypton 版本里 GlobalPaletteMode 是【实例属性】（不是静态属性），
        // 所以只能"先有一个 manager 对象、再改它的 GlobalPaletteMode"。
        // Krypton 约定：全程序只有一个"活动"的 KryptonManager 决定皮肤；这里用静态字段
        // 长期强引用这一个实例（防止被 GC），改它就等于改全局主题，两个页面一起刷新。
        static Krypton.Toolkit.KryptonManager? _manager;
        static void EnsureManager()
        {
            if (_manager == null)
                _manager = new Krypton.Toolkit.KryptonManager();
        }

        // 下拉框候选主题（按友好顺序展示）。运行期用 Enum.GetNames 过滤出当前 Krypton 版本真实存在的成员，
        // 因此不会因为版本枚举名差异而崩或漏项。
        static readonly string[] PreferredOrder =
        {
            "Global",
            "Sparkle4Blue", "Sparkle4DarkBlue", "Sparkle4Silver", "Sparkle4White",
            "Office2010Blue", "Office2010Silver", "Office2010Black",
            "Office2013Light", "Office2013DarkGray", "Office2013Black",
            "Microsoft365Blue", "Microsoft365Silver", "Microsoft365Black", "Microsoft365White",
        };

        public static List<string> GetThemeNames()
        {
            var valid = new HashSet<string>(Enum.GetNames<PaletteMode>(), StringComparer.OrdinalIgnoreCase);
            var list = new List<string>();
            foreach (var n in PreferredOrder)
                if (valid.Contains(n)) list.Add(n);
            return list;
        }

        // ---- 主题下拉框注册表（弱引用）：让多窗体的下拉框互相联动 ----
        static readonly List<WeakReference<Krypton.Toolkit.KryptonComboBox>> _combos = new();
        static bool _syncing;

        /// <summary>在 Application.Run 之前调用一次。</summary>
        public static void Initialize() => Apply(ThemeName);

        /// <summary>按主题名切换（字符串安全解析，解析不到就保持当前值）。</summary>
        public static void Apply(string themeName)
        {
            if (Enum.TryParse<PaletteMode>(themeName, true, out var mode))
                Apply(mode);
        }

        /// <summary>按枚举切换：全局调色板刷新 + 所有已注册下拉框同步选中。</summary>
        public static void Apply(PaletteMode mode)
        {
            Mode = mode;
            EnsureManager();
            _manager!.GlobalPaletteMode = mode;

            // 同步所有已注册的主题下拉框（防抖，避免触发它们的 SelectedChanged 递归回 Apply）。
            _syncing = true;
            _combos.RemoveAll(r => !r.TryGetTarget(out var c) || c.IsDisposed);
            foreach (var wr in _combos)
                if (wr.TryGetTarget(out var c) && !c.IsDisposed)
                    SetComboSelection(c);
            _syncing = false;

            ThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        /// <summary>
        /// 绑定一个主题下拉框：填充候选项、回显当前主题、接管其选择事件。
        /// 窗体只需在构造函数 InitializeComponent() 之后调用一次 AppTheme.BindCombo(cmbTheme)。
        /// </summary>
        public static void BindCombo(Krypton.Toolkit.KryptonComboBox combo)
        {
            if (combo == null) return;

            combo.Items.Clear();
            foreach (var n in GetThemeNames()) combo.Items.Add(n);

            combo.SelectedIndexChanged -= ComboChanged;
            combo.SelectedIndexChanged += ComboChanged;

            _combos.RemoveAll(r => !r.TryGetTarget(out var c) || c.IsDisposed);
            _combos.Add(new WeakReference<Krypton.Toolkit.KryptonComboBox>(combo));

            SetComboSelection(combo);
        }

        static int IndexOfCurrent(Krypton.Toolkit.KryptonComboBox combo)
        {
            var cur = CurrentName;
            for (int i = 0; i < combo.Items.Count; i++)
                if (string.Equals(combo.Items[i]?.ToString(), cur, StringComparison.OrdinalIgnoreCase))
                    return i;
            return -1;
        }

        static void SetComboSelection(Krypton.Toolkit.KryptonComboBox combo)
        {
            int i = IndexOfCurrent(combo);
            if (i >= 0 && combo.SelectedIndex != i) combo.SelectedIndex = i;
        }

        // 用户在下拉框里选择主题时调用。
        static void ComboChanged(object? sender, EventArgs e)
        {
            if (_syncing) return;
            if (sender is Krypton.Toolkit.KryptonComboBox c && c.SelectedItem != null)
                Apply(c.SelectedItem.ToString()!);
        }
    }
}