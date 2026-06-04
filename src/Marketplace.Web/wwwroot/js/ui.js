token = '';
role = '';
userId = '';

function navigateTo(page) {
    document.querySelectorAll('.page').forEach(p => p.classList.remove('active'));
    const target = document.getElementById(`page-${page}`);
    if (target) target.classList.add('active');
    updateNavigationButtons(page);
    
    setTimeout(() => {
        if (page === 'catalog') {
            if (typeof loadProducts === 'function') loadProducts();
            if (typeof loadCategoryFilter === 'function') loadCategoryFilter();
        } else if (page === 'cart') {
            if (typeof renderCartPage === 'function') renderCartPage();
        } else if (page === 'orders') {
            if (typeof renderOrdersPage === 'function') renderOrdersPage();
        } else if (page === 'admin') {
            if (typeof loadUsers === 'function') loadUsers();
        } else if (page === 'categories') {
            setTimeout(() => {
                if (typeof loadCategoriesAdmin === 'function') loadCategoriesAdmin();
            }, 50);
        } else if (page === 'profile') {
            if (typeof loadProfile === 'function') loadProfile();
        }
    }, 100);
}

function updateNavigationButtons(activePage) {
    const navAuth = document.getElementById('navAuthBtn');
    const navLogout = document.getElementById('navLogoutBtn');
    const navCart = document.getElementById('navCartBtn');
    const navOrders = document.getElementById('navOrdersBtn');
    const navAdmin = document.getElementById('navAdminBtn');
    const navCategories = document.getElementById('navCategoriesBtn');
    const userBtn = document.getElementById('userInfo');
    const createBtn = document.getElementById('btnCreateProduct');

    if (token) {
        navAuth.classList.add('hidden');
        navLogout.classList.remove('hidden');
        if (role === 'Customer') navCart.classList.remove('hidden');
        else navCart.classList.add('hidden');
        navOrders.classList.remove('hidden');
        if (role === 'Admin') {
            navAdmin.classList.remove('hidden');
            if (navCategories) navCategories.classList.remove('hidden');
        } else {
            navAdmin.classList.add('hidden');
            if (navCategories) navCategories.classList.add('hidden');
        }
        userBtn.classList.remove('hidden');
        userBtn.textContent = role;
        if (createBtn) {
            if (role === 'Seller' || role === 'Admin') createBtn.classList.remove('hidden');
            else createBtn.classList.add('hidden');
        }
    } else {
        navAuth.classList.remove('hidden');
        navLogout.classList.add('hidden');
        navCart.classList.add('hidden');
        navOrders.classList.add('hidden');
        navAdmin.classList.add('hidden');
        if (navCategories) navCategories.classList.add('hidden');
        userBtn.classList.add('hidden');
        if (createBtn) createBtn.classList.add('hidden');
    }
}

async function restoreSession() {
    const savedToken = localStorage.getItem('token');
    const savedRole = localStorage.getItem('role');
    const savedUserId = localStorage.getItem('userId');
    if (savedToken && savedRole && savedUserId) {
        try {
            const response = await fetch('/api/auth/me', {
                headers: { 'Authorization': 'Bearer ' + savedToken }
            });
            if (response.ok) {
                token = savedToken;
                role = savedRole;
                userId = savedUserId;
                navigateTo('catalog');
                return;
            } else {
                localStorage.clear();
            }
        } catch (e) {
            console.error('Ошибка восстановления сессии', e);
        }
    }
    navigateTo('auth');
}

function logout() {
    token = '';
    role = '';
    userId = '';
    localStorage.clear();
    updateNavigationButtons('catalog');
    navigateTo('auth');
}

function loadCategoriesAdmin() {
    console.log('loadCategoriesAdmin вызвана');
    const tableBody = document.querySelector('#categoryTable tbody');
    if (!tableBody) {
        console.error('Таблица #categoryTable не найдена');
        return;
    }
    tableBody.innerHTML = 'Загрузка...';
    fetch('/api/categories')
        .then(res => res.json())
        .then(cats => {
            tableBody.innerHTML = '';
            if (!Array.isArray(cats) || cats.length === 0) {
                tableBody.innerHTML = '<tr><td colspan="3">Нет категорий</td></tr>';
                return;
            }
            cats.forEach(c => {
                tableBody.innerHTML += `<tr>
                    <td id="catName-${c.id}">${c.name}</td>
                    <td>${c.slug}</td>
                    <td>
                        <button onclick="startEditCategory('${c.id}')">✏️</button>
                        <button onclick="deleteCategory('${c.id}')" style="color:red;">❌</button>
                    </td>
                </tr>`;
            });
        })
        .catch(e => {
            console.error('Ошибка загрузки категорий', e);
            tableBody.innerHTML = '<tr><td colspan="3">Ошибка загрузки</td></tr>';
        });
}
