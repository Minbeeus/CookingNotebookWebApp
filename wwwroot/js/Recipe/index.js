document.addEventListener("DOMContentLoaded", () => {
    const categoryItems = document.querySelectorAll("#categoryList li");
    const timeItems = document.querySelectorAll("#timeList li");
    const gridViewBtn = document.getElementById("gridView");
    const listViewBtn = document.getElementById("listView");
    const recipeList = document.querySelector(".recipe-list");
    const modal = document.getElementById("recipeModal");
    const closeModal = document.querySelector(".close-modal");
    const searchInput = document.getElementById("searchInput");
    const searchBtn = document.getElementById("searchBtn");

    let cards = document.querySelectorAll(".recipe-card");

    // ====== Hàm cập nhật sự kiện click mở modal ======
    function refreshCards() {
        cards = document.querySelectorAll(".recipe-card");
        cards.forEach(card => {
            card.addEventListener("click", () => {
                document.getElementById("modalImage").src = card.dataset.image;
                document.getElementById("modalTitle").textContent = card.dataset.title;
                document.getElementById("modalAuthor").textContent = card.dataset.author || "Ẩn danh";
                document.getElementById("modalPrepTime").textContent = card.dataset.preptime + " phút";
                document.getElementById("modalCookTime").textContent = card.dataset.cooktime + " phút";
                document.getElementById("modalServings").textContent = card.dataset.servings;
                document.getElementById("modalDescription").textContent = card.dataset.description || "Chưa có mô tả chi tiết.";
                modal.style.display = "block";
            });
        });
    }

    refreshCards();

    // ====== Bộ lọc theo loại ======
    categoryItems.forEach(item => {
        item.addEventListener("click", () => {
            categoryItems.forEach(i => i.classList.remove("active"));
            item.classList.add("active");

            filterRecipes();
        });
    });

    // ====== Bộ lọc theo thời gian ======
    timeItems.forEach(item => {
        item.addEventListener("click", () => {
            timeItems.forEach(i => i.classList.remove("active"));
            item.classList.add("active");

            filterRecipes();
        });
    });

    // ====== Hàm lọc công thức ======
    function filterRecipes() {
        const activeCategory = document.querySelector("#categoryList li.active").dataset.category.toLowerCase();
        const activeTime = document.querySelector("#timeList li.active").dataset.time.toLowerCase();

        cards.forEach(card => {
            const cardCat = (card.dataset.category || "").toLowerCase();
            const cardTime = parseInt(card.dataset.time) || 0;

            let categoryMatch = activeCategory === "all" || cardCat === activeCategory;
            let timeMatch = true;

            if (activeTime !== "all") {
                switch (activeTime) {
                    case "15":
                        timeMatch = cardTime < 15;
                        break;
                    case "30":
                        timeMatch = cardTime >= 15 && cardTime <= 30;
                        break;
                    case "60":
                        timeMatch = cardTime > 30 && cardTime <= 60;
                        break;
                    case "120":
                        timeMatch = cardTime > 60;
                        break;
                }
            }

            const match = categoryMatch && timeMatch;
            card.style.display = match ? "block" : "none";
        });
    }

    // ====== Chuyển đổi dạng hiển thị ======
    gridViewBtn.addEventListener("click", () => {
        gridViewBtn.classList.add("active");
        listViewBtn.classList.remove("active");
        recipeList.classList.add("grid-view");
        recipeList.classList.remove("list-view");
    });

    listViewBtn.addEventListener("click", () => {
        listViewBtn.classList.add("active");
        gridViewBtn.classList.remove("active");
        recipeList.classList.add("list-view");
        recipeList.classList.remove("grid-view");
    });

    // ====== Tìm kiếm ======
    function performSearch() {
        const searchTerm = searchInput.value.toLowerCase().trim();

        cards.forEach(card => {
            const title = (card.dataset.title || "").toLowerCase();
            const description = (card.dataset.description || "").toLowerCase();
            const author = (card.dataset.author || "").toLowerCase();

            const match = title.includes(searchTerm) ||
                         description.includes(searchTerm) ||
                         author.includes(searchTerm);

            card.style.display = match ? "block" : "none";
        });
    }

    searchInput.addEventListener("input", performSearch);
    searchBtn.addEventListener("click", performSearch);

    // ====== Đóng modal ======
    closeModal.addEventListener("click", () => {
        modal.style.display = "none";
    });

    window.addEventListener("click", (event) => {
        if (event.target === modal) {
            modal.style.display = "none";
        }
    });
});
