function applyTheme(theme) {
    if (theme === 'system') {
        document.documentElement.removeAttribute('data-theme');
        localStorage.removeItem('theme');
    } else {
        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem('theme', theme);
    }
}

function toggleTheme() {
    const current = getCurrentTheme();
    let next;
    if (current === 'light') next = 'dark';
    else if (current === 'dark') next = 'system';
    else next = 'light';
    applyTheme(next);
    updateThemeIcon(next);
}

function getCurrentTheme() {
    const stored = localStorage.getItem('theme');
    if (stored) return stored;
    return 'system';
}

function updateThemeIcon(theme) {
    const btn = document.getElementById('themeSwitcher');
    if (!btn) return;
    if (theme === 'dark') btn.textContent = '🌙 Тёмная';
    else if (theme === 'light') btn.textContent = '☀️ Светлая';
    else btn.textContent = '🌓 Системная';
}

document.addEventListener('DOMContentLoaded', () => {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        applyTheme(savedTheme);
        updateThemeIcon(savedTheme);
    } else {
        applyTheme('system');
        updateThemeIcon('system');
    }
});
