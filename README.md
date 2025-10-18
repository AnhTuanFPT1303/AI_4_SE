🛳️ AI4SE Battleship Challenge
🧩 Giới thiệu

Dự án Battleship AI được phát triển cho cuộc thi AI4SE, với mục tiêu xây dựng một tác tử AI có khả năng chơi game Battleship một cách thông minh và chiến lược.
Người chơi sẽ đối đầu với bot AI trong trò chơi đánh tàu kinh điển, nơi mỗi đội sở hữu 4 con tàu với kích thước cố định.

⚙️ Luật chơi
1. Bố cục đội tàu
Mỗi đội (AI hoặc người chơi) có 4 tàu, với kích thước như sau:

Battleship	5 ô
Cruiser	4 ô	
Destroyer	3 ô

Các tàu được đặt trên bản đồ 10x10, theo chiều ngang hoặc dọc, không được chồng lấp.

2. Cách chơi
Hai bên lần lượt bắn vào một tọa độ trên bản đồ của đối thủ.
Nếu bắn trúng, ô đó được đánh dấu là hit (X), nếu trượt là miss (O).
Khi tất cả các ô của một tàu bị bắn trúng, tàu đó bị chìm.
Đội nào làm chìm toàn bộ tàu đối phương trước sẽ thắng.

🧠 Về AI Bot — Gemini Powered
🔹 Mô tả tổng quan
Bot AI trong dự án được điều khiển bởi Google Gemini, một mô hình ngôn ngữ đa phương thức mạnh mẽ, giúp AI:
Phân tích dữ liệu trận đấu theo ngữ cảnh (các lượt bắn trước, mô hình xác suất, trạng thái bản đồ).
Đưa ra quyết định chiến lược: xác định khu vực có khả năng cao chứa tàu địch.
Học từ kết quả trước đó để điều chỉnh hành vi tấn công và phòng thủ.
🔹 Cách hoạt động
Quan sát: bot ghi nhận kết quả các lượt bắn trước (hit/miss, vị trí).
Phân tích: dữ liệu được gửi qua prompt đến Gemini để mô hình dự đoán bước đi tiếp theo.
Hành động: Gemini trả về tọa độ “tối ưu”, bot thực hiện lệnh bắn.
Thích ứng: kết quả được đưa lại cho mô hình, giúp cải thiện quyết định ở lượt sau.

🚀 Cách chạy chương trình
1. Cài đặt môi trường
git clone https://github.com/AnhTuanFPT1303/AI_4_SE.git
