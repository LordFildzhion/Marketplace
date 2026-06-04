let currentProduct = null;
let currentImageIndex = 0;
let currentDetailProductId = null;
let imageZoomLevel = 1;

let isDragging = false;
let startX, startY, translateX = 0, translateY = 0;

function openImageViewer(url) {
    const viewer = document.getElementById('imageViewer');
    const img = document.getElementById('imageViewerImg');
    if (!viewer || !img) return;
    img.src = url;
    viewer.style.display = 'flex';
    imageZoomLevel = 1;
    translateX = 0;
    translateY = 0;
    img.style.transform = 'scale(1)';
    img.style.cursor = 'grab';

    img.onmousedown = initDrag;
    img.ondragstart = () => false;
    document.addEventListener('mousemove', dragImage);
    document.addEventListener('mouseup', stopDrag);
}

function closeImageViewer() {
    document.getElementById('imageViewer').style.display = 'none';
    document.removeEventListener('mousemove', dragImage);
    document.removeEventListener('mouseup', stopDrag);
}

function initDrag(e) {
    isDragging = true;
    startX = e.clientX - translateX;
    startY = e.clientY - translateY;
    const img = document.getElementById('imageViewerImg');
    img.style.cursor = 'grabbing';
    e.preventDefault();
}

function dragImage(e) {
    if (!isDragging) return;
    translateX = e.clientX - startX;
    translateY = e.clientY - startY;
    const img = document.getElementById('imageViewerImg');
    img.style.transform = `scale(${imageZoomLevel}) translate(${translateX / imageZoomLevel}px, ${translateY / imageZoomLevel}px)`;
}

function stopDrag() {
    isDragging = false;
    const img = document.getElementById('imageViewerImg');
    if (img) img.style.cursor = 'grab';
}

function zoomImage(delta) {
    imageZoomLevel += delta;
    if (imageZoomLevel < 0.5) imageZoomLevel = 0.5;
    if (imageZoomLevel > 5) imageZoomLevel = 5;
    const img = document.getElementById('imageViewerImg');
    img.style.transform = `scale(${imageZoomLevel}) translate(${translateX / imageZoomLevel}px, ${translateY / imageZoomLevel}px)`;
}

function resetZoom() {
    imageZoomLevel = 1;
    translateX = 0;
    translateY = 0;
    const img = document.getElementById('imageViewerImg');
    img.style.transform = 'scale(1)';
    img.style.cursor = 'grab';
}

async function openProductDetail(productId) {
    try {
        const response = await fetch(`/api/products/${productId}`);
        if (!response.ok) throw new Error('Товар не найден');
        currentProduct = await response.json();
        currentImageIndex = 0;
        currentDetailProductId = productId;

        document.getElementById('detailTitle').textContent = currentProduct.title;
        document.getElementById('detailCategory').textContent = currentProduct.categoryName || '—';
        document.getElementById('detailPrice').textContent = `$${currentProduct.price}`;
        document.getElementById('detailDescription').textContent = currentProduct.description;

        const stockContainer = document.getElementById('detailStock');
        stockContainer.innerHTML = '';
        if (role === 'Seller' || role === 'Admin') {
            const stockInput = document.createElement('input');
            stockInput.type = 'number';
            stockInput.id = 'stockDelta';
            stockInput.value = 1;
            stockInput.style.width = '60px';
            stockInput.style.marginRight = '5px';
            const applyBtn = document.createElement('button');
            applyBtn.textContent = 'Применить';
            applyBtn.onclick = async () => {
                const delta = parseInt(stockInput.value);
                if (isNaN(delta)) return;
                await adjustStock(currentProduct.id, delta);
            };
            const incBtn = document.createElement('button');
            incBtn.textContent = '+';
            incBtn.onclick = () => adjustStock(currentProduct.id, 1);
            const decBtn = document.createElement('button');
            decBtn.textContent = '-';
            decBtn.onclick = () => adjustStock(currentProduct.id, -1);
            stockContainer.appendChild(document.createTextNode(`Текущий остаток: ${currentProduct.stock} `));
            stockContainer.appendChild(incBtn);
            stockContainer.appendChild(decBtn);
            stockContainer.appendChild(stockInput);
            stockContainer.appendChild(applyBtn);
        } else {
            stockContainer.textContent = currentProduct.stock;
        }

        renderGallery();

        const actionsDiv = document.getElementById('detailActions');
        actionsDiv.innerHTML = '';
        if (role === 'Customer') {
            actionsDiv.innerHTML += `<button onclick="addToCart('${currentProduct.id}',1)">🛒 Добавить в корзину</button>`;
        }
        if (role === 'Seller' || role === 'Admin') {
            actionsDiv.innerHTML += `<button onclick="startEditProduct()">✏️ Редактировать</button>`;
        }
        if (role) {
            actionsDiv.innerHTML += `<button onclick="openDetailReviews()">📝 Отзывы</button>`;
        }
        if (role === 'Seller' || role === 'Admin') {
            actionsDiv.innerHTML += `<button onclick="document.getElementById('imageUpload_${currentProduct.id}').click()">🖼️ Добавить фото</button>
                                    <input type="file" id="imageUpload_${currentProduct.id}" accept="image/*" style="display:none" onchange="uploadImage('${currentProduct.id}', this)">`;
            if (currentProduct.images && currentProduct.images.length > 0) {
                actionsDiv.innerHTML += `<button onclick="deleteCurrentImage()">🗑️ Удалить это фото</button>`;
            }
        }

        document.getElementById('detailReviewsPanel').style.display = 'none';
        document.getElementById('productDetailModal').style.display = 'block';
    } catch (e) {
        alert('Ошибка загрузки информации о товаре');
        console.error(e);
    }
}

async function adjustStock(productId, delta) {
    if (!token) return;
    try {
        const response = await fetch(`/api/products/${productId}/stock`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify({ delta })
        });
        if (response.ok) {
            openProductDetail(productId);
            if (typeof loadProducts === 'function') await loadProducts();
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

function renderGallery() {
    const container = document.getElementById('detailImages');
    container.innerHTML = '';
    if (!currentProduct.images || currentProduct.images.length === 0) {
        container.innerHTML = '<em>Нет изображений</em>';
        return;
    }

    const images = currentProduct.images;
    if (currentImageIndex >= images.length) currentImageIndex = images.length - 1;
    if (currentImageIndex < 0) currentImageIndex = 0;

    const mainImg = document.createElement('img');
    mainImg.src = images[currentImageIndex].url;
    mainImg.style.maxWidth = '100%';
    mainImg.style.maxHeight = '300px';
    mainImg.style.display = 'block';
    mainImg.style.marginBottom = '10px';
    mainImg.onclick = () => openImageViewer(images[currentImageIndex].url);
    container.appendChild(mainImg);

    if (images.length > 1) {
        const navDiv = document.createElement('div');
        navDiv.style.display = 'flex';
        navDiv.style.justifyContent = 'center';
        navDiv.style.gap = '10px';
        navDiv.style.marginBottom = '10px';
        const prevBtn = document.createElement('button');
        prevBtn.textContent = '←';
        prevBtn.onclick = () => { currentImageIndex = (currentImageIndex - 1 + images.length) % images.length; renderGallery(); };
        const nextBtn = document.createElement('button');
        nextBtn.textContent = '→';
        nextBtn.onclick = () => { currentImageIndex = (currentImageIndex + 1) % images.length; renderGallery(); };
        navDiv.appendChild(prevBtn);
        navDiv.appendChild(nextBtn);
        container.appendChild(navDiv);

        const thumbsDiv = document.createElement('div');
        thumbsDiv.style.display = 'flex';
        thumbsDiv.style.gap = '5px';
        thumbsDiv.style.justifyContent = 'center';
        images.forEach((img, idx) => {
            const thumb = document.createElement('img');
            thumb.src = img.url;
            thumb.style.width = '50px';
            thumb.style.height = '50px';
            thumb.style.objectFit = 'cover';
            thumb.style.cursor = 'pointer';
            thumb.style.border = idx === currentImageIndex ? '2px solid blue' : '1px solid #ccc';
            thumb.onclick = () => { currentImageIndex = idx; renderGallery(); openImageViewer(img.url); };
            thumbsDiv.appendChild(thumb);
        });
        container.appendChild(thumbsDiv);
    }
}

async function deleteCurrentImage() {
    if (!currentProduct || !currentProduct.images || currentProduct.images.length === 0) return;
    const img = currentProduct.images[currentImageIndex];
    if (!img) return;
    if (!confirm('Удалить это изображение?')) return;
    try {
        const response = await fetch(`/api/products/${currentProduct.id}/images/${img.id}`, {
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) {
            currentProduct.images.splice(currentImageIndex, 1);
            if (currentProduct.images.length === 0) currentImageIndex = 0;
            else if (currentImageIndex >= currentProduct.images.length) currentImageIndex = currentProduct.images.length - 1;
            renderGallery();
            if (typeof loadProducts === 'function') await loadProducts();
            if (currentProduct.images.length === 0) {
                document.getElementById('detailActions').querySelector('button[onclick="deleteCurrentImage()"]')?.remove();
            }
        } else {
            const err = await response.json();
            alert('Ошибка удаления: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

function openDetailReviews() {
    if (!currentDetailProductId) return;
    const panel = document.getElementById('detailReviewsPanel');
    panel.style.display = 'flex';
    loadDetailReviews(currentDetailProductId);
}

function closeDetailReviews() {
    document.getElementById('detailReviewsPanel').style.display = 'none';
}

async function loadDetailReviews(productId) {
    const container = document.getElementById('detailReviewsContent');
    container.innerHTML = 'Загрузка...';
    try {
        const response = await fetch(`/api/reviews/product/${productId}`);
        if (!response.ok) throw new Error('Ошибка загрузки');
        const reviews = await response.json();
        if (!reviews.length) {
            container.innerHTML = '<p>Отзывов пока нет.</p>';
            return;
        }
        let html = '';
        reviews.forEach(r => {
            const canEdit = (userId === r.userId) && role === 'Customer';
            const canDelete = (role === 'Admin') || (role === 'Customer' && userId === r.userId);
            const canRespond = (role === 'Seller' || role === 'Admin') && !r.sellerResponse;
            const canDeleteResponse = role === 'Admin' && r.sellerResponse;

            let imagesHtml = '';
            if (r.imageUrls && r.imageUrls.length > 0) {
                imagesHtml = '<div style="display:flex; gap:5px; margin:5px 0;">';
                r.imageUrls.forEach(url => {
                    imagesHtml += `<img src="${url}" style="width:50px; height:50px; object-fit:cover; border:1px solid #ccc; cursor:pointer;" onclick="openImageViewer('${url}')">`;
                });
                imagesHtml += '</div>';
            }

            html += `<div style="border-bottom:1px solid var(--table-border); padding:10px 0;">
                <strong>${r.userName || 'Аноним'}</strong> (${r.rating}/5)
                <div>${r.comment}</div>
                ${imagesHtml}
                <small>${new Date(r.createdAt).toLocaleString()}</small>
                ${r.sellerResponse ? `<div style="color:darkgreen;"><em>Ответ: ${r.sellerResponse}</em></div>` : ''}
                <div style="margin-top:5px;">
                    ${canEdit ? `<button onclick="editDetailReview('${r.id}')">Ред.</button>` : ''}
                    ${canDelete ? `<button onclick="deleteDetailReview('${r.id}')">Удал.</button>` : ''}
                    ${canRespond ? `<button onclick="respondToDetailReview('${r.id}')">Ответить</button>` : ''}
                    ${canDeleteResponse ? `<button onclick="deleteDetailResponse('${r.id}')">Удал. ответ</button>` : ''}
                </div>
            </div>`;
        });
        container.innerHTML = html;
    } catch (e) {
        container.innerHTML = '<span class="error">Ошибка загрузки отзывов</span>';
    }
}

async function submitDetailReview() {
    if (!currentDetailProductId) return;
    const rating = parseInt(document.getElementById('detailReviewRating').value);
    const comment = document.getElementById('detailReviewComment').value;
    if (!rating || !comment) return alert('Заполните рейтинг и комментарий');

    const formData = new FormData();
    formData.append('rating', rating);
    formData.append('comment', comment);
    const files = document.getElementById('reviewImages').files;
    for (const file of files) formData.append('images', file);

    try {
        const response = await fetch(`/api/reviews/product/${currentDetailProductId}`, {
            method: 'POST',
            headers: { 'Authorization': 'Bearer ' + token },
            body: formData
        });
        if (response.ok) {
            document.getElementById('detailReviewComment').value = '';
            document.getElementById('reviewImages').value = '';
            loadDetailReviews(currentDetailProductId);
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

async function editDetailReview(reviewId) {
    const newComment = prompt('Новый текст отзыва:');
    if (!newComment) return;
    await fetch(`/api/reviews/${reviewId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
        body: JSON.stringify({ comment: newComment })
    });
    if (currentDetailProductId) loadDetailReviews(currentDetailProductId);
}

async function deleteDetailReview(reviewId) {
    if (!confirm('Удалить отзыв?')) return;
    await fetch(`/api/reviews/${reviewId}`, {
        method: 'DELETE',
        headers: { 'Authorization': 'Bearer ' + token }
    });
    if (currentDetailProductId) loadDetailReviews(currentDetailProductId);
}

async function respondToDetailReview(reviewId) {
    const responseText = prompt('Введите ответ:');
    if (!responseText) return;
    await fetch(`/api/reviews/${reviewId}/response`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
        body: JSON.stringify({ response: responseText })
    });
    if (currentDetailProductId) loadDetailReviews(currentDetailProductId);
}

async function deleteDetailResponse(reviewId) {
    if (!confirm('Удалить ответ?')) return;
    await fetch(`/api/reviews/${reviewId}/response`, {
        method: 'DELETE',
        headers: { 'Authorization': 'Bearer ' + token }
    });
    if (currentDetailProductId) loadDetailReviews(currentDetailProductId);
}

function startEditProduct() {
    if (!currentProduct) return;
    const titleEl = document.getElementById('detailTitle');
    const descEl = document.getElementById('detailDescription');
    const priceEl = document.getElementById('detailPrice');
    const categoryEl = document.getElementById('detailCategory');

    titleEl.innerHTML = `<input type="text" id="editTitle" value="${currentProduct.title}" style="width:100%;">`;
    descEl.innerHTML = `<textarea id="editDescription" style="width:100%;">${currentProduct.description}</textarea>`;
    priceEl.innerHTML = `<input type="number" id="editPrice" value="${currentProduct.price}" step="0.01" style="width:100%;">`;
    categoryEl.innerHTML = `<select id="editCategory" style="width:100%;"></select>`;

    fetch('/api/categories')
        .then(res => res.json())
        .then(categories => {
            const select = document.getElementById('editCategory');
            categories.forEach(cat => {
                const option = document.createElement('option');
                option.value = cat.id;
                option.textContent = cat.name;
                select.appendChild(option);
            });
            select.value = currentProduct.categoryId;
        });

    const actionsDiv = document.getElementById('detailActions');
    const editBtn = actionsDiv.querySelector('button[onclick="startEditProduct()"]');
    if (editBtn) editBtn.remove();
    actionsDiv.innerHTML = `<button onclick="saveProductChanges()">💾 Сохранить</button>
                           <button onclick="cancelEdit()">❌ Отмена</button>` + actionsDiv.innerHTML;
}

async function saveProductChanges() {
    if (!currentProduct || !token) return;
    const title = document.getElementById('editTitle').value.trim();
    const description = document.getElementById('editDescription').value.trim();
    const price = parseFloat(document.getElementById('editPrice').value);
    const categoryId = document.getElementById('editCategory')?.value || null;

    if (!title || !description || isNaN(price)) {
        alert('Заполните все поля корректно');
        return;
    }

    const body = { title, description, price };
    if (categoryId) body.categoryId = categoryId;

    try {
        const response = await fetch(`/api/products/${currentProduct.id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify(body)
        });
        if (response.ok) {
            const updated = await response.json();
            currentProduct = updated;
            openProductDetail(currentProduct.id);
            if (typeof loadProducts === 'function') await loadProducts();
        } else {
            const err = await response.json();
            alert('Ошибка сохранения: ' + (err.message || err.code));
        }
    } catch(e) {
        alert('Ошибка сети');
    }
}

function cancelEdit() {
    openProductDetail(currentProduct.id);
}
