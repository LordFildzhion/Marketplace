async function loadProfile() {
    if (!token) return;
    const container = document.getElementById('profileContent');
    container.innerHTML = 'Загрузка...';
    try {
        const response = await fetch('/api/auth/me', {
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (!response.ok) throw new Error('Ошибка загрузки профиля');
        const user = await response.json();
        container.innerHTML = `
            <p><strong>Имя:</strong> ${user.firstName} ${user.lastName}</p>
            <p><strong>Email:</strong> ${user.email}</p>
            <p><strong>Роль:</strong> ${user.role}</p>
        `;
    } catch (e) {
        container.innerHTML = '<span class="error">Ошибка загрузки профиля</span>';
    }
}
