async function renderOrdersPage() {
    const container = document.getElementById('ordersPageContent');
    if (!container) return;
    container.innerHTML = 'Загрузка...';
    try {
        const response = await fetch('/api/orders', {
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (!response.ok) throw new Error('Ошибка загрузки заказов');
        const orders = await response.json();
        if (!orders.length) {
            container.innerHTML = '<p>Заказов пока нет.</p>';
            return;
        }

        let html = '';
        for (const o of orders) {
            const totalItems = o.items?.reduce((sum, item) => sum + item.quantity, 0) || 0;

            let buyerInfo = '';
            if ((role === 'Seller' || role === 'Admin') && o.buyerName) {
                buyerInfo = `<br><em>Покупатель: ${o.buyerName} (${o.buyerEmail})</em>`;
            }
            let sellerInfo = '';
            if (role === 'Admin' && o.sellerNames) {
                sellerInfo = `<br><em>Продавцы: ${o.sellerNames}</em>`;
            }

            let itemsHtml = '';
            if (o.items && o.items.length) {
                itemsHtml = '<div class="product-grid" style="margin-top:10px;">';
                for (const item of o.items) {
                    // item.product.imageUrls теперь массив строк
                    const imageUrl = (item.product && item.product.imageUrls && item.product.imageUrls.length > 0)
                        ? item.product.imageUrls[0]
                        : '';

                    const card = `
                        <div class="product-card" onclick="openProductDetail('${item.productId}')" style="margin:0;">
                            <img src="${imageUrl || 'data:image/svg+xml,%3Csvg xmlns=%27http://www.w3.org/2000/svg%27 width=%27200%27 height=%27160%27%3E%3Crect fill=%27%23ddd%27 width=%27200%27 height=%27160%27/%3E%3Ctext x=%2750%25%27 y=%2750%25%27 dominant-baseline=%27middle%27 text-anchor=%27middle%27 fill=%27%23999%27%3ENo Image%3C/text%3E%3C/svg%3E'}"
                                 alt="${item.productTitle}" style="height:120px;">
                            <div class="product-card-body">
                                <div class="product-card-title">${item.productTitle}</div>
                                <div class="product-card-price">Цена: $${item.unitPrice.toFixed(2)}</div>
                                <div style="margin:5px 0;">Кол-во: ${item.quantity}</div>
                                <div style="font-weight:bold;">Сумма: $${item.totalPrice.toFixed(2)}</div>
                            </div>
                        </div>`;
                    itemsHtml += card;
                }
                itemsHtml += '</div>';
            } else {
                itemsHtml = 'Нет товаров';
            }

            html += `<div class="order-card">
                <strong>Заказ #${o.id.substring(0,8)}</strong><br>
                <em>Статус: ${o.status}</em>${buyerInfo}${sellerInfo}<br>
                <em>Сумма: $${o.totalAmount.toFixed(2)}</em><br>
                <em>Дата: ${new Date(o.orderDate).toLocaleString()}</em><br>
                <details>
                    <summary>Товары (${totalItems} ед.)</summary>
                    ${itemsHtml}
                </details>
            </div>`;
        }
        container.innerHTML = html;
    } catch (e) {
        container.innerHTML = '<span class="error">Ошибка загрузки заказов</span>';
    }
}
