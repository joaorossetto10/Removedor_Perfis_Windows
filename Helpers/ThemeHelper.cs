using RemovedorPerfisWindows.Models;

namespace RemovedorPerfisWindows.Helpers;

public sealed class ThemePalette
{
    public required Color FormBackColor { get; init; }
    public required Color PanelBackColor { get; init; }
    public required Color InputBackColor { get; init; }
    public required Color TextColor { get; init; }
    public required Color MutedTextColor { get; init; }
    public required Color TitleColor { get; init; }
    public required Color AccentColor { get; init; }
    public required Color PrimaryButtonBackColor { get; init; }
    public required Color PrimaryButtonForeColor { get; init; }
    public required Color PrimaryButtonHoverBackColor { get; init; }
    public required Color PrimaryButtonPressedBackColor { get; init; }
    public required Color PrimaryButtonBorderColor { get; init; }
    public required Color SecondaryButtonBackColor { get; init; }
    public required Color SecondaryButtonForeColor { get; init; }
    public required Color SecondaryButtonHoverBackColor { get; init; }
    public required Color SecondaryButtonPressedBackColor { get; init; }
    public required Color SecondaryButtonBorderColor { get; init; }
    public required Color CriticalButtonBackColor { get; init; }
    public required Color CriticalButtonForeColor { get; init; }
    public required Color CriticalButtonHoverBackColor { get; init; }
    public required Color CriticalButtonPressedBackColor { get; init; }
    public required Color CriticalButtonBorderColor { get; init; }
    public required Color DisabledButtonBackColor { get; init; }
    public required Color DisabledButtonForeColor { get; init; }
    public required Color DisabledButtonBorderColor { get; init; }
    public required Color GridBackColor { get; init; }
    public required Color GridForeColor { get; init; }
    public required Color GridHeaderBackColor { get; init; }
    public required Color GridHeaderForeColor { get; init; }
    public required Color GridLineColor { get; init; }
    public required Color GridSelectionBackColor { get; init; }
    public required Color GridSelectionForeColor { get; init; }
    public required Color LogBackColor { get; init; }
    public required Color LogForeColor { get; init; }
    public required Color AvailableRowColor { get; init; }
    public required Color SelectedForActionRowColor { get; init; }
    public required Color ProtectedRowColor { get; init; }
    public required Color AttentionRowColor { get; init; }
    public required Color BlockedRowColor { get; init; }
    public required Color RemovedRowColor { get; init; }
}

public static class ThemeHelper
{
    public static ThemePalette GetPalette(AppThemeMode mode)
    {
        return mode == AppThemeMode.Dark ? DarkPalette : LightPalette;
    }

    private static ThemePalette LightPalette => new()
    {
        FormBackColor = ColorTranslator.FromHtml("#F9FAFB"),
        PanelBackColor = Color.White,
        InputBackColor = Color.White,
        TextColor = ColorTranslator.FromHtml("#111827"),
        MutedTextColor = ColorTranslator.FromHtml("#374151"),
        TitleColor = ColorTranslator.FromHtml("#1E3A8A"),
        AccentColor = ColorTranslator.FromHtml("#D4AF37"),
        PrimaryButtonBackColor = ColorTranslator.FromHtml("#1E3A8A"),
        PrimaryButtonForeColor = Color.White,
        PrimaryButtonHoverBackColor = ColorTranslator.FromHtml("#2563EB"),
        PrimaryButtonPressedBackColor = ColorTranslator.FromHtml("#1D4ED8"),
        PrimaryButtonBorderColor = ColorTranslator.FromHtml("#1E3A8A"),
        SecondaryButtonBackColor = ColorTranslator.FromHtml("#F9FAFB"),
        SecondaryButtonForeColor = ColorTranslator.FromHtml("#1E3A8A"),
        SecondaryButtonHoverBackColor = ColorTranslator.FromHtml("#E5E7EB"),
        SecondaryButtonPressedBackColor = ColorTranslator.FromHtml("#DBEAFE"),
        SecondaryButtonBorderColor = ColorTranslator.FromHtml("#1E3A8A"),
        CriticalButtonBackColor = ColorTranslator.FromHtml("#D4AF37"),
        CriticalButtonForeColor = ColorTranslator.FromHtml("#111827"),
        CriticalButtonHoverBackColor = ColorTranslator.FromHtml("#F5D76E"),
        CriticalButtonPressedBackColor = ColorTranslator.FromHtml("#B8941F"),
        CriticalButtonBorderColor = ColorTranslator.FromHtml("#B8941F"),
        DisabledButtonBackColor = ColorTranslator.FromHtml("#E5E7EB"),
        DisabledButtonForeColor = ColorTranslator.FromHtml("#6B7280"),
        DisabledButtonBorderColor = ColorTranslator.FromHtml("#CBD5E1"),
        GridBackColor = Color.White,
        GridForeColor = ColorTranslator.FromHtml("#111827"),
        GridHeaderBackColor = ColorTranslator.FromHtml("#DBEAFE"),
        GridHeaderForeColor = ColorTranslator.FromHtml("#1E3A8A"),
        GridLineColor = ColorTranslator.FromHtml("#E5E7EB"),
        GridSelectionBackColor = ColorTranslator.FromHtml("#DBEAFE"),
        GridSelectionForeColor = ColorTranslator.FromHtml("#111827"),
        LogBackColor = Color.White,
        LogForeColor = ColorTranslator.FromHtml("#111827"),
        AvailableRowColor = Color.White,
        SelectedForActionRowColor = ColorTranslator.FromHtml("#DBEAFE"),
        ProtectedRowColor = ColorTranslator.FromHtml("#E5E7EB"),
        AttentionRowColor = ColorTranslator.FromHtml("#FEF3C7"),
        BlockedRowColor = ColorTranslator.FromHtml("#FEE2E2"),
        RemovedRowColor = ColorTranslator.FromHtml("#DCFCE7")
    };

    private static ThemePalette DarkPalette => new()
    {
        FormBackColor = ColorTranslator.FromHtml("#0F172A"),
        PanelBackColor = ColorTranslator.FromHtml("#1E293B"),
        InputBackColor = ColorTranslator.FromHtml("#0F172A"),
        TextColor = ColorTranslator.FromHtml("#F8FAFC"),
        MutedTextColor = ColorTranslator.FromHtml("#CBD5E1"),
        TitleColor = ColorTranslator.FromHtml("#F5D76E"),
        AccentColor = ColorTranslator.FromHtml("#D4AF37"),
        PrimaryButtonBackColor = ColorTranslator.FromHtml("#2563EB"),
        PrimaryButtonForeColor = Color.White,
        PrimaryButtonHoverBackColor = ColorTranslator.FromHtml("#1D4ED8"),
        PrimaryButtonPressedBackColor = ColorTranslator.FromHtml("#1E40AF"),
        PrimaryButtonBorderColor = ColorTranslator.FromHtml("#2563EB"),
        SecondaryButtonBackColor = ColorTranslator.FromHtml("#1E293B"),
        SecondaryButtonForeColor = ColorTranslator.FromHtml("#F8FAFC"),
        SecondaryButtonHoverBackColor = ColorTranslator.FromHtml("#334155"),
        SecondaryButtonPressedBackColor = ColorTranslator.FromHtml("#0F172A"),
        SecondaryButtonBorderColor = ColorTranslator.FromHtml("#2563EB"),
        CriticalButtonBackColor = ColorTranslator.FromHtml("#D4AF37"),
        CriticalButtonForeColor = ColorTranslator.FromHtml("#111827"),
        CriticalButtonHoverBackColor = ColorTranslator.FromHtml("#F5D76E"),
        CriticalButtonPressedBackColor = ColorTranslator.FromHtml("#B8941F"),
        CriticalButtonBorderColor = ColorTranslator.FromHtml("#D4AF37"),
        DisabledButtonBackColor = ColorTranslator.FromHtml("#374151"),
        DisabledButtonForeColor = ColorTranslator.FromHtml("#CBD5E1"),
        DisabledButtonBorderColor = ColorTranslator.FromHtml("#475569"),
        GridBackColor = ColorTranslator.FromHtml("#1E293B"),
        GridForeColor = ColorTranslator.FromHtml("#F8FAFC"),
        GridHeaderBackColor = ColorTranslator.FromHtml("#1E3A8A"),
        GridHeaderForeColor = Color.White,
        GridLineColor = ColorTranslator.FromHtml("#475569"),
        GridSelectionBackColor = ColorTranslator.FromHtml("#2563EB"),
        GridSelectionForeColor = Color.White,
        LogBackColor = ColorTranslator.FromHtml("#020617"),
        LogForeColor = ColorTranslator.FromHtml("#F8FAFC"),
        AvailableRowColor = ColorTranslator.FromHtml("#1E293B"),
        SelectedForActionRowColor = ColorTranslator.FromHtml("#2563EB"),
        ProtectedRowColor = ColorTranslator.FromHtml("#334155"),
        AttentionRowColor = ColorTranslator.FromHtml("#5A4717"),
        BlockedRowColor = ColorTranslator.FromHtml("#4B1D24"),
        RemovedRowColor = ColorTranslator.FromHtml("#14532D")
    };
}
