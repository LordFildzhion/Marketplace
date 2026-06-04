async function login() {
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;
    const statusDiv = document.getElementById('loginStatus');
    try {
        const response = await fetch('/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });
        const data = await response.json();
        if (response.ok) {
            token = data.accessToken;
            role = data.user.role;
            userId = data.user.id;
            localStorage.setItem('token', token);
            localStorage.setItem('role', role);
            localStorage.setItem('userId', userId);
            document.getElementById('userInfo').innerHTML = `Вы вошли как ${role}`;
            updateNavigationButtons('catalog');
            navigateTo('catalog');
            if (typeof loadProducts === 'function') loadProducts();
            if (typeof loadCategoryFilter === 'function') loadCategoryFilter();
        } else {
            statusDiv.innerHTML = '<span class="error">' + (data.message || 'Ошибка входа') + '</span>';
        }
    } catch (e) {
        statusDiv.innerHTML = '<span class="error">Ошибка сети</span>';
    }
}

async function register() {
    const firstName = document.getElementById('regFirstName').value.trim();
    const lastName = document.getElementById('regLastName').value.trim();
    const email = document.getElementById('regEmail').value.trim();
    const password = document.getElementById('regPassword').value;
    const confirmPassword = document.getElementById('regConfirmPassword').value;
    const roleSelect = document.getElementById('regRoleSelect').value;
    const statusDiv = document.getElementById('registerStatus');

    if (!firstName || !lastName || !email || !password) {
        statusDiv.innerHTML = '<span class="error">Все поля обязательны</span>';
        return;
    }
    if (password !== confirmPassword) {
        statusDiv.innerHTML = '<span class="error">Пароли не совпадают</span>';
        return;
    }

    try {
        const response = await fetch('/api/auth/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                firstName, lastName, email, password, confirmPassword,
                Role: roleSelect
            })
        });
        const data = await response.json();
        if (response.ok) {
            statusDiv.innerHTML = '<span class="success">Регистрация успешна! Теперь войдите.</span>';
            document.getElementById('regFirstName').value = '';
            document.getElementById('regLastName').value = '';
            document.getElementById('regEmail').value = '';
            document.getElementById('regPassword').value = '';
            document.getElementById('regConfirmPassword').value = '';
            setTimeout(() => { navigateTo('catalog'); statusDiv.innerHTML = ''; }, 1500);
        } else {
            statusDiv.innerHTML = '<span class="error">' + (data.message || data.code) + '</span>';
        }
    } catch (e) {
        statusDiv.innerHTML = '<span class="error">Ошибка сети</span>';
    }
}
