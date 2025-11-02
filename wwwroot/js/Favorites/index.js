document.addEventListener("DOMContentLoaded", () => {
const typeOfDishList = document.getElementById("typeOfDishList");
const cookingMethodList = document.getElementById("cookingMethodList");
const timeList = document.getElementById("timeList");
const gridViewBtn = document.getElementById("gridView");
const listViewBtn = document.getElementById("listView");
const recipeList = document.querySelector(".recipe-list");
const searchInput = document.getElementById("searchInput");
const searchBtn = document.getElementById("searchBtn");
const clearFiltersBtn = document.getElementById("clearFilters");

let cards = document.querySelectorAll(".recipe-card");

// ====== Hàm cập nhật sự kiện click mở modal ======
function refreshCards() {
cards = document.querySelectorAll(".recipe-card");
}

refreshCards();

// ====== Bộ lọc ======
function addFilterListeners(listElement, filterName) {
    const lis = listElement.querySelectorAll("li");
    lis.forEach(li => {
        li.addEventListener("click", () => {
            // Remove active from siblings
            lis.forEach(sib => sib.classList.remove("active"));
            // Add active to clicked
            li.classList.add("active");
            // Filter
            filterRecipes();
        });
    });
}

addFilterListeners(typeOfDishList, "typeOfDishFilter");
addFilterListeners(cookingMethodList, "cookingMethodFilter");
addFilterListeners(timeList, "timeFilter");

// ====== Hàm lọc công thức ======
function filterRecipes() {
const typeOfDish = typeOfDishList.querySelector(".active").dataset.value;
const cookingMethod = cookingMethodList.querySelector(".active").dataset.value;
const timeFilterValue = timeList.querySelector(".active").dataset.value;

// Redirect to server with filters
const url = '/Favorites/Index?' +
'typeOfDishFilter=' + encodeURIComponent(typeOfDish) +
'&cookingMethodFilter=' + encodeURIComponent(cookingMethod) +
'&timeFilter=' + encodeURIComponent(timeFilterValue);
window.location.href = url;
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

// ====== Xoá lọc ======
clearFiltersBtn.addEventListener("click", () => {
    // Redirect to Favorites/Index without any filters
    window.location.href = '/Favorites/Index';
});
});
