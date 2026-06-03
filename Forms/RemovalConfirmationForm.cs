using RemovedorPerfisWindows.Controls;
using RemovedorPerfisWindows.Helpers;
using RemovedorPerfisWindows.Models;

namespace RemovedorPerfisWindows.Forms;

public partial class RemovalConfirmationForm : Form
{
    private readonly string _computerName;
    private AppThemeMode _themeMode;

    public RemovalConfirmationForm(string computerName, IReadOnlyList<UserProfileInfo> profiles, AppThemeMode themeMode)
    {
        _computerName = computerName;
        _themeMode = themeMode;
        InitializeComponent();
        lblComputerName.Text = $"Computador remoto: {computerName}";
        lblProfileCount.Text = $"Perfis selecionados: {profiles.Count}";

        foreach (var profile in profiles)
        {
            lstProfiles.Items.Add($"{profile.UserName} | {profile.Sid} | {profile.LocalPath}");
        }

        ApplyTheme(themeMode);
    }

    private void TxtConfirmation_TextChanged(object? sender, EventArgs e)
    {
        btnConfirm.Enabled = string.Equals(
            txtConfirmation.Text.Trim(),
            _computerName,
            StringComparison.OrdinalIgnoreCase);
        ApplyButtonTheme(btnConfirm, ThemeHelper.GetPalette(_themeMode), critical: true);
    }

    private void BtnConfirm_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void ApplyTheme(AppThemeMode themeMode)
    {
        var palette = ThemeHelper.GetPalette(themeMode);

        BackColor = palette.FormBackColor;
        ForeColor = palette.TextColor;

        foreach (Control control in Controls)
        {
            ApplyControlTheme(control, palette);
        }

        lblTitle.ForeColor = palette.TitleColor;
        lblTitle.Font = new Font(lblTitle.Font, FontStyle.Bold);
        lblWarning.ForeColor = palette.MutedTextColor;

        ApplyButtonTheme(btnConfirm, palette, critical: true);
        ApplyButtonTheme(btnCancel, palette, critical: false);
    }

    private static void ApplyControlTheme(Control control, ThemePalette palette)
    {
        switch (control)
        {
            case Label label:
                label.BackColor = Color.Transparent;
                label.ForeColor = palette.TextColor;
                break;
            case TextBox textBox:
                textBox.BackColor = palette.InputBackColor;
                textBox.ForeColor = palette.TextColor;
                textBox.BorderStyle = BorderStyle.FixedSingle;
                break;
            case ListBox listBox:
                listBox.BackColor = palette.InputBackColor;
                listBox.ForeColor = palette.TextColor;
                listBox.BorderStyle = BorderStyle.FixedSingle;
                break;
        }
    }

    private static void ApplyButtonTheme(Button button, ThemePalette palette, bool critical)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;

        if (!button.Enabled)
        {
            button.BackColor = palette.DisabledButtonBackColor;
            button.ForeColor = palette.DisabledButtonForeColor;
            button.FlatAppearance.BorderColor = palette.DisabledButtonBorderColor;
            button.FlatAppearance.MouseOverBackColor = palette.DisabledButtonBackColor;
            button.FlatAppearance.MouseDownBackColor = palette.DisabledButtonBackColor;
            ApplyThemedButtonColors(
                button,
                palette.DisabledButtonBackColor,
                palette.DisabledButtonForeColor,
                palette.DisabledButtonBorderColor,
                palette.DisabledButtonBackColor,
                palette.DisabledButtonBackColor,
                palette);
            button.UseVisualStyleBackColor = false;
            return;
        }

        var backColor = critical ? palette.CriticalButtonBackColor : palette.SecondaryButtonBackColor;
        var foreColor = critical ? palette.CriticalButtonForeColor : palette.SecondaryButtonForeColor;
        var borderColor = critical ? palette.CriticalButtonBorderColor : palette.SecondaryButtonBorderColor;
        var hoverBackColor = critical ? palette.CriticalButtonHoverBackColor : palette.SecondaryButtonHoverBackColor;
        var pressedBackColor = critical ? palette.CriticalButtonPressedBackColor : palette.SecondaryButtonPressedBackColor;

        button.BackColor = backColor;
        button.ForeColor = foreColor;
        button.FlatAppearance.BorderColor = borderColor;
        button.FlatAppearance.MouseOverBackColor = hoverBackColor;
        button.FlatAppearance.MouseDownBackColor = pressedBackColor;
        button.UseVisualStyleBackColor = false;

        ApplyThemedButtonColors(button, backColor, foreColor, borderColor, hoverBackColor, pressedBackColor, palette);
    }

    private static void ApplyThemedButtonColors(
        Button button,
        Color backColor,
        Color foreColor,
        Color borderColor,
        Color hoverBackColor,
        Color pressedBackColor,
        ThemePalette palette)
    {
        if (button is not ThemedButton themedButton)
        {
            return;
        }

        themedButton.BackColor = backColor;
        themedButton.ForeColor = foreColor;
        themedButton.ButtonBorderColor = borderColor;
        themedButton.HoverBackColor = hoverBackColor;
        themedButton.PressedBackColor = pressedBackColor;
        themedButton.DisabledBackColor = palette.DisabledButtonBackColor;
        themedButton.DisabledForeColor = palette.DisabledButtonForeColor;
        themedButton.DisabledBorderColor = palette.DisabledButtonBorderColor;
        themedButton.Invalidate();
    }
}
