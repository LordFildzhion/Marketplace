let currentProductId = null;

async function openReviews(productId) {
    currentProductId = productId;
    await loadReviews();
    document.getElementById('reviewsModal').style.display = 'block';
}

async function loadReviews() {
    if (!currentProductId) return;
    const container = document.getElementById('reviewsList');
    container.innerHTML = 'Загрузка...';
    try {
        const response = await fetch(`/api/reviews/product/${currentProductId}`);
        const reviews = await response.json();
        if (!reviews.length) {
            container.innerHTML = '<p>Отзывов пока нет.</p>';
            return;
        }
        let html = '';
        for (const r of reviews) {
            const canEdit = (userId === r.userId) && role === 'Customer';
            const canDelete = (role === 'Admin') || (role === 'Customer' && userId === r.userId);
            const canRespond = (role === 'Seller' || role === 'Admin') && !r.sellerResponse;
            const canDeleteResponse = role === 'Admin' && r.sellerResponse;

            html += `<div style="border:1px solid #ddd; margin:10px 0; padding:10px;">
                <strong>${r.userName}</strong> (${r.rating}/5)
                <div>${r.comment}</div>
                <small>${new Date(r.createdAt).toLocaleString()}</small>
                ${r.sellerResponse ? `<div style="margin-top:5px; color:darkgreen;"><em>Ответ: ${r.sellerResponse}</em></div>` : ''}
                <div style="margin-top:5px;">
                    ${canEdit ? `<button onclick="editReview('${r.id}')">Редактировать</button>` : ''}
                    ${canDelete ? `<button onclick="deleteReview('${r.id}')">Удалить</button>` : ''}
                    ${canRespond ? `<button onclick="respondToReview('${r.id}')">Ответить</button>` : ''}
                    ${canDeleteResponse ? `<button onclick="deleteResponse('${r.id}')">Удалить ответ</button>` : ''}
                </div>
            </div>`;
        }
        container.innerHTML = html;
    } catch (e) {
        container.innerHTML = '<span class="error">Ошибка загрузки отзывов</span>';
    }
}

async function submitReview() {
    const rating = parseInt(document.getElementById('reviewRating').value);
    const comment = document.getElementById('reviewComment').value;
    if (!rating || !comment) return alert('Заполните рейтинг и комментарий');
    try {
        const response = await fetch(`/api/reviews/product/${currentProductId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify({ rating, comment })
        });
        if (response.ok) {
            loadReviews();
            document.getElementById('reviewComment').value = '';
        } else {
            const err = await response.json();
            alert('Ошибка: ' + (err.message || err.code));
        }
    } catch(e) { alert('Ошибка сети'); }
}

async function editReview(reviewId) {
    const newComment = prompt('Введите новый текст отзыва:');
    if (!newComment) return;
    try {
        const response = await fetch(`/api/reviews/${reviewId}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify({ comment: newComment })
        });
        if (response.ok) loadReviews();
        else alert('Ошибка редактирования');
    } catch(e) { alert('Ошибка сети'); }
}

async function deleteReview(reviewId) {
    if (!confirm('Удалить отзыв?')) return;
    try {
        const response = await fetch(`/api/reviews/${reviewId}`, {
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) loadReviews();
        else alert('Не удалось удалить');
    } catch(e) { alert('Ошибка сети'); }
}

async function respondToReview(reviewId) {
    const responseText = prompt('Введите текст ответа:');
    if (!responseText) return;
    try {
        const response = await fetch(`/api/reviews/${reviewId}/response`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'Authorization': 'Bearer ' + token },
            body: JSON.stringify({ response: responseText })
        });
        if (response.ok) loadReviews();
        else alert('Ошибка при ответе');
    } catch(e) { alert('Ошибка сети'); }
}

async function deleteResponse(reviewId) {
    if (!confirm('Удалить ответ?')) return;
    try {
        const response = await fetch(`/api/reviews/${reviewId}/response`, {
            method: 'DELETE',
            headers: { 'Authorization': 'Bearer ' + token }
        });
        if (response.ok) loadReviews();
        else alert('Не удалось удалить ответ');
    } catch(e) { alert('Ошибка сети'); }
}
