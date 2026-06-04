
let selectedModalFiles = [];

if (typeof window.productsLoading === 'undefined') window.productsLoading = false;
if (typeof window.productsLoaded === 'undefined') window.productsLoaded = false;

async function loadModalCategories() {
    const select = document.getElementById('modalCategorySelect');
    if (!select) return;
    select.innerHTML = '';
    try {
        const response = await fetch('/api/categories');
        const categories = await response.json();
        categories.forEach(cat => {
            const option = document.createElement('option');
            option.value = cat.id;
            option.textContent = cat.name;
            select.appendChild(option);
        });
    } catch (e) {
        console.error('Ошибка загрузки категорий', e);
    }
}

async function loadCategoryFilter() {
    const select = document.getElementById('categoryFilter');
    if (!select) return;
    select.innerHTML = '<option value="">Все категории</option>';
    try {
        const response = await fetch('/api/categories');
        const categories = await response.json();
        categories.forEach(cat => {
            const option = document.createElement('option');
            option.value = cat.id;
            option.textContent = cat.name;
            select.appendChild(option);
        });
    } catch (e) {
        console.error('Ошибка загрузки категорий', e);
    }
}

function searchProducts() {
    const categoryId = document.getElementById('categoryFilter')?.value || '';
    window.productsLoaded = false; // разрешаем перезагрузку при поиске
    loadProducts(null, categoryId);
}

function resetFilters() {
    const searchInput = document.getElementById('searchQuery');
    const categorySelect = document.getElementById('categoryFilter');
    if (searchInput) searchInput.value = '';
    if (categorySelect) categorySelect.value = '';
    window.productsLoaded = false; // разрешаем перезагрузку при сбросе
    loadProducts();
}

async function loadProducts(queryOverride, categoryIdOverride) {
    if (window.productsLoading) {
        console.log('loadProducts уже выполняется, пропуск');
        return;
    }

    if (window.productsLoaded && !queryOverride && !categoryIdOverride) {
        console.log('Товары уже загружены, повторная загрузка не требуется');
        return;
    }

    window.productsLoading = true;

    const query = queryOverride ?? (document.getElementById('searchQuery')?.value || '');
    const categoryId = categoryIdOverride ?? (document.getElementById('categoryFilter')?.value || '');
    const container = document.getElementById('productContainer');
    if (!container) {
        window.productsLoading = false;
        return;
    }

    if (container.offsetParent === null) {
        window.productsLoading = false;
        return;
    }

    container.innerHTML = '';
    try {
        const url = new URL('/api/products', window.location.origin);
        if (query) url.searchParams.set('query', query);
        if (categoryId) url.searchParams.set('categoryId', categoryId);
        const response = await fetch(url);
        const data = await response.json();
        if (data.items && data.items.length) {
            data.items.forEach(p => {
                const card = document.createElement('div');
                card.className = 'product-card';
                card.setAttribute('data-product-id', p.id);

                const img = document.createElement('img');
                if (p.images && p.images.length > 0) {
                    img.src = p.images[0].url;
                } else {
                    img.src = 'data:image/svg+xml,%3Csvg xmlns="http://www.w3.org/2000/svg" width="200" height="160" viewBox="0 0 200 160"%3E%3Crect fill="%23ddd" width="200" height="160"/%3E%3Ctext fill="%23999" font-family="Arial" font-size="16" x="50%" y="50%" dominant-baseline="middle" text-anchor="middle"%3ENo Image%3C/text%3E%3C/svg%3E';
                }
                img.alt = p.title;
                card.appendChild(img);

                const body = document.createElement('div');
                body.className = 'product-card-body';

                const title = document.createElement('div');
                title.className = 'product-card-title';
                title.textContent = p.title;
                body.appendChild(title);

                const price = document.createElement('div');
                price.className = 'product-card-price';
                price.textContent = `$${p.price.toFixed(2)}`;
                body.appendChild(price);

                const rating = document.createElement('div');
                rating.className = 'product-card-rating';
                const stars = Math.round(p.averageRating);
                rating.innerHTML = '★'.repeat(stars) + '☆'.repeat(5 - stars) + ` ${p.averageRating.toFixed(1)}`;
                body.appendChild(rating);

                const stock = document.createElement('div');
                stock.className = 'product-card-stock';
                stock.textContent = `Остаток: ${p.stock}`;
                body.appendChild(stock);

                const seller = document.createElement('div');
                seller.className = 'product-card-seller';
                seller.textContent = `Продавец: ${p.sellerName || "Неизвестный"}`;
                body.appendChild(seller);

                const actionsDiv = document.createElement('div');
                actionsDiv.style.display = 'flex';
                actionsDiv.style.gap = '5px';
                actionsDiv.style.marginTop = '10px';
                actionsDiv.style.justifyContent = 'center';

                if (role === 'Customer') {
                    const addBtn = document.createElement('button');
                    addBtn.textContent = '🛒';
                    addBtn.onclick = (e) => { e.stopPropagation(); addToCart(p.id, 1); };
                    actionsDiv.appendChild(addBtn);
                } else if (role === 'Seller' || role === 'Admin') {
                    const delBtn = document.createElement('button');
                    delBtn.textContent = '❌';
                    delBtn.style.color = 'red';
                    delBtn.onclick = (e) => { e.stopPropagation(); deleteProduct(p.id); };
                    actionsDiv.appendChild(delBtn);
                }

                if (role) {
                    const detailBtn = document.createElement('button');
                    detailBtn.textContent = 'ℹ️';
                    detailBtn.onclick = (e) => { e.stopPropagation(); openProductDetail(p.id); };
                    actionsDiv.appendChild(detailBtn);
                }

                body.appendChild(actionsDiv);
                card.appendChild(body);
                card.addEventListener('click', () => openProductDetail(p.id));
                container.appendChild(card);
            });
            document.getElementById('productListStatus').innerHTML = '';
            window.productsLoaded = true;
        } else {
            container.innerHTML = '<p>Товаров пока нет.</p>';
            window.productsLoaded = true;
        }
    } catch (e) {
        container.innerHTML = '<span class="error">Ошибка загрузки товаров</span>';
    } finally {
        window.productsLoading = false;
    }
}

async function deleteProduct(productId) {
    if (!token) return;
    if (!confirm('Удалить товар навсегда?')) return;
    try {
        const response = await fetch('/api/products/' + productId, {
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) {
            const card = document.querySelector(`.product-card[data-product-id="${productId}"]`);
            if (card) card.remove();
        } else {
            const err = await response.json();
            alert('Ошибка удаления: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

async function uploadImage(productId, input) {
    if (!token) return;
    const file = input.files[0];
    if (!file) return;
    const formData = new FormData();
    formData.append('file', file);
    try {
        const response = await fetch(`/api/products/${productId}/images`, {
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token },
            body: formData
        });
        if (response.ok) {
            window.productsLoaded = false;
            loadProducts();
        } else {
            const err = await response.json();
            alert('Ошибка загрузки: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

async function deleteImage(productId, imageId) {
    if (!token) return;
    if (!confirm('Удалить изображение?')) return;
    try {
        const response = await fetch(`/api/products/${productId}/images/${imageId}`, {
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) {
            window.productsLoaded = false;
            loadProducts();
        } else {
            const err = await response.json();
            alert('Ошибка удаления: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

function openCreateProductModal() {
    loadModalCategories();
    document.getElementById('modalTitle').value = '';
    document.getElementById('modalDescription').value = '';
    document.getElementById('modalPrice').value = '99.99';
    document.getElementById('modalStock').value = '10';
    document.getElementById('modalProductStatus').innerHTML = '';
    document.getElementById('modalImageFiles').value = '';
    document.getElementById('modalImagePreview').innerHTML = '';
    selectedModalFiles = [];
    document.getElementById('createProductModal').style.display = 'block';
}

function previewModalImages() {
    const input = document.getElementById('modalImageFiles');
    const preview = document.getElementById('modalImagePreview');
    preview.innerHTML = '';
    selectedModalFiles = Array.from(input.files);
    selectedModalFiles.forEach((file, index) => {
        const reader = new FileReader();
        reader.onload = function(e) {
            const imgDiv = document.createElement('div');
            imgDiv.style.position = 'relative';
            imgDiv.style.display = 'inline-block';
            const img = document.createElement('img');
            img.src = e.target.result;
            img.style.width = '60px';
            img.style.height = '60px';
            img.style.objectFit = 'cover';
            img.style.border = '1px solid #ccc';
            const delBtn = document.createElement('span');
            delBtn.textContent = '×';
            delBtn.style.position = 'absolute';
            delBtn.style.top = '0';
            delBtn.style.right = '0';
            delBtn.style.background = 'red';
            delBtn.style.color = 'white';
            delBtn.style.cursor = 'pointer';
            delBtn.style.padding = '0 2px';
            delBtn.onclick = function() { removeModalImage(index); };
            imgDiv.appendChild(img);
            imgDiv.appendChild(delBtn);
            preview.appendChild(imgDiv);
        };
        reader.readAsDataURL(file);
    });
}

function removeModalImage(index) {
    selectedModalFiles.splice(index, 1);
    const dt = new DataTransfer();
    selectedModalFiles.forEach(file => dt.items.add(file));
    document.getElementById('modalImageFiles').files = dt.files;
    previewModalImages();
}

async function createProductFromModal() {
    if (!token) return;
    const categoryId = document.getElementById('modalCategorySelect').value;
    const product = {
        sku: document.getElementById('modalSku')?.value || '',
        title: document.getElementById('modalTitle').value,
        description: document.getElementById('modalDescription').value,
        price: parseFloat(document.getElementById('modalPrice').value),
        stock: parseInt(document.getElementById('modalStock').value),
        categoryId: categoryId
    };
    try {
        const response = await fetch('/api/products', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify(product)
        });
        const data = await response.json();
        if (response.ok) {
            document.getElementById('modalProductStatus').innerHTML = '<span class="success">Товар создан! Загружаем фото...</span>';
            for (const file of selectedModalFiles) {
                const formData = new FormData();
                formData.append('file', file);
                await fetch(`/api/products/${data.id}/images`, {
                    method: 'POST',
                    headers: { 'Authorization': 'Bearer ' + token },
                    body: formData
                });
            }
            setTimeout(() => {
                document.getElementById('createProductModal').style.display = 'none';
                window.productsLoaded = false;
                loadProducts();
            }, 1000);
        } else {
            document.getElementById('modalProductStatus').innerHTML = '<span class="error">' + (data.message || data.code) + '</span>';
        }
    } catch (e) {
        document.getElementById('modalProductStatus').innerHTML = '<span class="error">Ошибка сети</span>';
    }
}
