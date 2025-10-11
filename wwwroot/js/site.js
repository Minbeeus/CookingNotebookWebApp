document.addEventListener("DOMContentLoaded", () => {
    const userBtn = document.getElementById("userMenuBtn");
    const dropdown = document.getElementById("userDropdown");

    if (userBtn && dropdown) {
        // Toggle hiển thị menu khi click icon
        userBtn.addEventListener("click", (e) => {
            e.stopPropagation();
            dropdown.classList.toggle("show");
        });

        // Ẩn menu khi click ra ngoài
        document.addEventListener("click", () => {
            dropdown.classList.remove("show");
        });
    } else {
        console.warn("⚠️ Không tìm thấy userMenuBtn hoặc userDropdown trong DOM!");
    }
});