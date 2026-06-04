async function addToCart(productId, quantity) {
    if (!token || role !== 'Customer') {
        alert('Войдите как покупатель');
        return;
    }
    try {
        const response = await fetch('/api/cart', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify({ productId, quantity })
        });
        if (response.ok) {
            navigateTo('cart');
            renderCartPage();
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

async function renderCartPage() {
    const container = document.getElementById('cartPageContent');
    if (!container) return;
    container.innerHTML = 'Загрузка...';
    try {
        const response = await fetch('/api/cart', {
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (!response.ok) throw new Error('Ошибка загрузки корзины');
        const items = await response.json();
        if (!items || items.length === 0) {
            container.innerHTML = '<p>Корзина пуста</p>';
            return;
        }

        let html = '';
        for (const item of items) {
            html += `<div style="border:1px solid #ccc; margin:10px 0; padding:10px; display:flex; gap:10px;">
                <img src="${item.imageUrl || 'https://via.placeholder.com/50'}" style="width:50px; height:50px; object-fit:cover;">
                <div style="flex:1;">
                    <strong>${item.productTitle}</strong>
                    <div>Цена: $${item.unitPrice.toFixed(2)}</div>
                    <div>Кол-во: ${item.quantity}</div>
                    <div>Сумма: $${item.totalPrice.toFixed(2)}</div>
                    <div style="display:flex; gap:5px; margin-top:5px;">
                        <button onclick="cartUpdateQuantity('${item.productId}', ${item.quantity - 1})">-</button>
                        <button onclick="cartUpdateQuantity('${item.productId}', ${item.quantity + 1})">+</button>
                        <button onclick="cartRemoveItem('${item.productId}')" style="color:red;">Удалить</button>
                    </div>
                </div>
            </div>`;
        }
        html += `<div style="margin-top:10px; text-align:right;">
            <button onclick="cartClear()">Очистить</button>
            <button onclick="cartCreateOrder()">Оформить заказ</button>
        </div>`;
        container.innerHTML = html;
    } catch (e) {
        container.innerHTML = '<span class="error">Ошибка загрузки корзины</span>';
    }
}

async function cartUpdateQuantity(productId, newQuantity) {
    if (newQuantity <= 0) {
        await cartRemoveItem(productId);
        return;
    }
    try {
        const response = await fetch(`/api/cart/${productId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify({ productId, quantity: newQuantity })
        });
        if (response.ok) {
            renderCartPage();
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

async function cartRemoveItem(productId) {
    if (!confirm('Удалить товар из корзины?')) return;
    try {
        const response = await fetch(`/api/cart/${productId}`, {
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) {
            renderCartPage();
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

async function cartClear() {
    if (!confirm('Очистить корзину?')) return;
    try {
        const response = await fetch('/api/cart', {
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) {
            renderCartPage();
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

async function cartCreateOrder() {
    try {
        const response = await fetch('/api/orders', {
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) {
            alert('Заказ создан!');
            renderCartPage();
            if (typeof loadProducts === 'function') await loadProducts();
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}
