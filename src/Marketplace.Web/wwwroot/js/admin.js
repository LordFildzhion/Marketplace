async function loadUsers() {
    if (role !== 'Admin') return;
    const tableBody = document.querySelector('#adminUserTable tbody');
    if (!tableBody) return;
    const search = document.getElementById('userSearch')?.value || '';
    const roleFilter = document.getElementById('userRoleFilter')?.value || '';
    tableBody.innerHTML = 'Загрузка...';
    try {
        const url = new URL('/api/admin/users', window.location.origin);
        url.searchParams.set('page', '1');
        url.searchParams.set('pageSize', '100');
        if (search) url.searchParams.set('search', search);
        if (roleFilter) url.searchParams.set('role', roleFilter);
        const response = await fetch(url, {
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (!response.ok) throw new Error('Ошибка сервера: ' + response.status);
        const text = await response.text();
        if (!text) throw new Error('Пустой ответ от сервера');
        const data = JSON.parse(text);
        if (data.items && data.items.length) {
            tableBody.innerHTML = '';
            data.items.forEach(user => {
                let actionHtml = '';
                if (user.role === 'Seller') {
                    actionHtml += user.isApproved
                        ? `<button onclick="disapproveSeller('${user.id}')">❌ Заблокировать</button>`
                        : `<button onclick="approveSeller('${user.id}')">✅ Одобрить</button>`;
                }
                actionHtml += `<button onclick="deleteUser('${user.id}')" style="color:red;">❌ Удалить</button>`;
                actionHtml += `<button onclick="resetPassword('${user.id}')">🔑 Сбросить пароль</button>`;
                const status = (user.role === 'Seller') ? (user.isApproved ? 'Одобрен' : 'Ожидает') : '-';
                const row = `<tr>
                    <td>${user.firstName} ${user.lastName}</td>
                    <td>${user.email}</td>
                    <td>${user.role} (${status})</td>
                    <td>${actionHtml}</td>
                </tr>`;
                tableBody.innerHTML += row;
            });
        } else {
            tableBody.innerHTML = '<tr><td colspan="4">Нет пользователей</td></tr>';
        }
    } catch (e) {
        console.error('Ошибка загрузки пользователей', e);
        tableBody.innerHTML = '<tr><td colspan="4">Ошибка загрузки</td></tr>';
    }
}

async function approveSeller(userId) {
    if (!token) return;
    try {
        const response = await fetch(`/api/admin/users/${userId}/approve`, {
            method: 'PATCH',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) { loadUsers(); }
        else { const err = await response.json(); alert('Ошибка: ' + (err.message || err.code)); }
    } catch (e) { alert('Ошибка сети'); }
}

async function disapproveSeller(userId) {
    if (!token) return;
    try {
        const response = await fetch(`/api/admin/users/${userId}/disapprove`, {
            method: 'PATCH',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) { loadUsers(); }
        else { const err = await response.json(); alert('Ошибка: ' + (err.message || err.code)); }
    } catch (e) { alert('Ошибка сети'); }
}

async function deleteUser(userId) {
    if (!token) return;
    if (!confirm('Удалить пользователя навсегда?')) return;
    try {
        const response = await fetch('/api/admin/users/' + userId, {
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) { loadUsers(); }
        else { const err = await response.json(); alert('Ошибка: ' + (err.message || err.code)); }
    } catch (e) { alert('Ошибка сети'); }
}

async function resetPassword(userId) {
    if (!token) return;
    if (!confirm('Сгенерировать новый пароль для этого пользователя?')) return;
    try {
        const response = await fetch(`/api/admin/users/${userId}/reset-password`, {
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) {
            const data = await response.json();
            alert('Новый пароль: ' + data.newPassword);
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch (e) { alert('Ошибка сети'); }
}

function openCreateCategoryModal() {
    document.getElementById('categoryName').value = '';
    document.getElementById('categoryStatus').innerHTML = '';
    const parentSelect = document.getElementById('categoryParent');
    parentSelect.innerHTML = '<option value="">Без родительской</option>';
    fetch('/api/categories')
        .then(response => response.json())
        .then(categories => {
            categories.forEach(cat => {
                const option = document.createElement('option');
                option.value = cat.id;
                option.textContent = cat.name;
                parentSelect.appendChild(option);
            });
        })
        .catch(e => console.error('Ошибка загрузки категорий', e));
    document.getElementById('createCategoryModal').style.display = 'block';
}

async function createCategory() {
    const name = document.getElementById('categoryName').value.trim();
    const parentSelect = document.getElementById('categoryParent');
    const parentCategoryId = parentSelect?.value || null;
    if (!name) {
        document.getElementById('categoryStatus').innerHTML = '<span class="error">Введите название</span>';
        return;
    }
    try {
        const body = { name };
        if (parentCategoryId) body.parentCategoryId = parentCategoryId;
        else body.parentCategoryId = null;
        const response = await fetch('/api/categories', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify(body)
        });
        if (response.ok) {
            document.getElementById('categoryStatus').innerHTML = '<span class="success">Категория создана!</span>';
            setTimeout(() => {
                document.getElementById('createCategoryModal').style.display = 'none';
                if (typeof loadModalCategories === 'function') loadModalCategories();
                if (typeof loadCategoryFilter === 'function') loadCategoryFilter();
                if (typeof loadCategoriesAdmin === 'function') loadCategoriesAdmin();
            }, 1000);
        } else {
            const err = await response.json();
            document.getElementById('categoryStatus').innerHTML = '<span class="error">Ошибка: ' + (err.message || err.code) + '</span>';
        }
    } catch (e) {
        document.getElementById('categoryStatus').innerHTML = '<span class="error">Ошибка сети</span>';
    }
}

function startEditCategory(id) {
    const cell = document.getElementById(`catName-${id}`);
    if (!cell) return;
    const oldName = cell.textContent;
    cell.innerHTML = `<input type="text" id="editCatName-${id}" value="${oldName}">`;
    const row = cell.closest('tr');
    if (row) {
        const actionsCell = row.querySelector('td:last-child');
        if (actionsCell) {
            actionsCell.innerHTML = `<button onclick="saveCategory('${id}')">💾</button><button onclick="cancelEditCategory('${id}', '${oldName}')">❌</button>`;
        }
    }
}

async function saveCategory(id) {
    const input = document.getElementById(`editCatName-${id}`);
    if (!input) return;
    const newName = input.value.trim();
    if (!newName) return;
    try {
        const response = await fetch(`/api/categories/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify({ name: newName })
        });
        if (response.ok) {
            loadCategoriesAdmin();
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

function cancelEditCategory(id, oldName) {
    const cell = document.getElementById(`catName-${id}`);
    if (!cell) return;
    cell.textContent = oldName;
    const row = cell.closest('tr');
    if (row) {
        const actionsCell = row.querySelector('td:last-child');
        if (actionsCell) {
            actionsCell.innerHTML = `<button onclick="startEditCategory('${id}')">✏️</button><button onclick="deleteCategory('${id}')" style="color:red;">❌</button>`;
        }
    }
}

async function deleteCategory(id) {
    if (!confirm('Удалить категорию?')) return;
    try {
        const response = await fetch(`/api/categories/${id}`, {
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) {
            loadCategoriesAdmin();
            if (typeof loadModalCategories === 'function') loadModalCategories();
            if (typeof loadCategoryFilter === 'function') loadCategoryFilter();
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}
