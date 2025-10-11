document.addEventListener("DOMContentLoaded", () => {
    const categoryItems = document.querySelectorAll("#categoryList li");
    const gridViewBtn = document.getElementById("gridView");
    const listViewBtn = document.getElementById("listView");
    const ingredientList = document.querySelector(".ingredient-list");
    const modal = document.getElementById("ingredientModal");
    const closeModal = document.querySelector(".close-modal");
    const searchInput = document.getElementById("searchInput");

    let cards = document.querySelectorAll(".ingredient-card");

    // ====== Hàm cập nhật sự kiện click mở modal ======
    function refreshCards() {
        cards = document.querySelectorAll(".ingredient-card");
        cards.forEach(card => {
            card.addEventListener("click", () => {
                document.getElementById("modalImage").src = card.dataset.image;
                document.getElementById("modalNameVi").textContent = card.dataset.namevi;
                document.getElementById("modalNameEn").textContent = card.dataset.nameen || "Đang cập nhật...";
                document.getElementById("modalCategory").textContent = card.dataset.category;
                document.getElementById("modalDescription").textContent = card.dataset.description || "Chưa có mô tả chi tiết.";
                modal.classList.add("active");
            });
        });
    }

    refreshCards();

    // ====== Bộ lọc theo loại ======
    categoryItems.forEach(item => {
        item.addEventListener("click", () => {
            categoryItems.forEach(i => i.classList.remove("active"));
            item.classList.add("active");

            const category = item.dataset.category.toLowerCase();
            cards.forEach(card => {
                const cardCat = (card.dataset.category || "").toLowerCase();
                const match = category === "all" || cardCat === category;
                card.style.display = match ? "flex" : "none";
            });
        });
    });

    // ====== Chuyển đổi dạng hiển thị ======
    gridViewBtn.addEventListener("click", () => {
        gridViewBtn.classList.add("active");
        listViewBtn.classList.remove("active");
        ingredientList.classList.add("grid-view");
        ingredientList.classList.remove("list-view");
    });

    listViewBtn.addEventListener("click", () => {
        listViewBtn.classList.add("active");
        gridViewBtn.classList.remove("active");
        ingredientList.classList.add("list-view");
        ingredientList.classList.remove("grid-view");
    });

    // ====== Đóng modal ======
    closeModal.addEventListener("click", () => modal.classList.remove("active"));
    modal.addEventListener("click", e => {
        if (e.target === modal) modal.classList.remove("active");
    });

    // ====== Hàm bỏ dấu tiếng Việt ======
    function removeAccents(str) {
        if (!str) return "";
        return str
            .normalize("NFD")
            .replace(/[\u0300-\u036f]/g, "")
            .replace(/đ/g, "d")
            .replace(/Đ/g, "D");
    }

    // ====== Tìm kiếm không dấu ======
    searchInput.addEventListener("input", () => {
        const keyword = removeAccents(searchInput.value.toLowerCase().trim());
        cards.forEach(card => {
            const nameRaw = (card.dataset.namevi || "").toLowerCase();
            const nameNoAccent = removeAccents(nameRaw);
            const match = nameNoAccent.includes(keyword);
            card.style.display = match ? "flex" : "none";
        });
    });
});