USE QuanLyBenhVienDb;
GO

-- Bác sĩ phẫu thuật
INSERT INTO Users (Username, Password, Role, FullName, Email, SDT) VALUES
('bs.phauthuat1', '123', 'PhauThuat', N'BS. Nguyễn Văn A', 'bs.a@benhvien.com', '0901234567'),
('bs.phauthuat2', '123', 'PhauThuat', N'BS. Trần Thị B', 'bs.b@benhvien.com', '0901234568'),
('bs.phauthuat3', '123', 'PhauThuat', N'BS. Lê Văn C', 'bs.c@benhvien.com', '0901234569'),
('bs.phauthuat4', '123', 'PhauThuat', N'BS. Phạm Thị D', 'bs.d@benhvien.com', '0901234570'),
('bs.phauthuat5', '123', 'PhauThuat', N'BS. Hoàng Văn E', 'bs.e@benhvien.com', '0901234571');
GO

-- 20 loại mổ
INSERT INTO HospitalFees (FeeCode, FeeName, Category, Price, InsuranceCoverage, Description, IsActive, CreatedAt) VALUES
('PT001', N'Phẫu thuật ruột thừa nội soi', N'Phẫu thuật', 5000000, 80, N'Cắt ruột thừa bằng phương pháp nội soi', 1, GETDATE()),
('PT002', N'Phẫu thuật nội soi túi mật', N'Phẫu thuật', 7000000, 80, N'Cắt bỏ túi mật qua nội soi', 1, GETDATE()),
('PT003', N'Phẫu thuật thoát vị bẹn', N'Phẫu thuật', 4500000, 80, N'Phục hồi thành bụng do thoát vị bẹn', 1, GETDATE()),
('PT004', N'Phẫu thuật trĩ Longo', N'Phẫu thuật', 8000000, 80, N'Cắt trĩ bằng phương pháp Longo', 1, GETDATE()),
('PT005', N'Phẫu thuật cắt dạ dày', N'Phẫu thuật', 15000000, 80, N'Cắt bỏ một phần hoặc toàn bộ dạ dày', 1, GETDATE()),
('PT006', N'Phẫu thuật thay khớp gối', N'Phẫu thuật', 12000000, 80, N'Thay thế khớp gối bị hỏng', 1, GETDATE()),
('PT007', N'Phẫu thuật thay khớp háng', N'Phẫu thuật', 14000000, 80, N'Thay thế khớp háng bằng khớp nhân tạo', 1, GETDATE()),
('PT008', N'Phẫu thuật kết hợp xương gãy', N'Phẫu thuật', 6000000, 80, N'Cố định xương gãy bằng đinh, nẹp vít', 1, GETDATE()),
('PT009', N'Phẫu thuật lấy thai lần 1', N'Phẫu thuật', 5500000, 80, N'Mổ đẻ lần 1', 1, GETDATE()),
('PT010', N'Phẫu thuật bóc u xơ tử cung', N'Phẫu thuật', 6500000, 80, N'Loại bỏ khối u xơ trong tử cung', 1, GETDATE()),
('PT011', N'Phẫu thuật cắt tử cung', N'Phẫu thuật', 9000000, 80, N'Cắt bỏ tử cung do bệnh lý', 1, GETDATE()),
('PT012', N'Phẫu thuật u nang buồng trứng', N'Phẫu thuật', 7500000, 80, N'Bóc tách hoặc cắt bỏ u nang buồng trứng', 1, GETDATE()),
('PT013', N'Phẫu thuật mộng thịt', N'Phẫu thuật', 1500000, 80, N'Cắt bỏ mộng thịt ở mắt', 1, GETDATE()),
('PT014', N'Phẫu thuật đục thủy tinh thể (Phaco)', N'Phẫu thuật', 4000000, 80, N'Thay thủy tinh thể nhân tạo', 1, GETDATE()),
('PT015', N'Phẫu thuật cắt amidan', N'Phẫu thuật', 3500000, 80, N'Cắt bỏ amidan viêm nhiễm', 1, GETDATE()),
('PT016', N'Phẫu thuật nội soi xoang mũi', N'Phẫu thuật', 8500000, 80, N'Điều trị viêm xoang qua nội soi', 1, GETDATE()),
('PT017', N'Phẫu thuật nạo VA', N'Phẫu thuật', 2000000, 80, N'Nạo bỏ tổ chức VA bị viêm', 1, GETDATE()),
('PT018', N'Phẫu thuật vá nhĩ', N'Phẫu thuật', 5000000, 80, N'Vá lại màng nhĩ bị rách', 1, GETDATE()),
('PT019', N'Phẫu thuật u tuyến giáp', N'Phẫu thuật', 10000000, 80, N'Cắt bỏ một phần hoặc toàn bộ tuyến giáp', 1, GETDATE()),
('PT020', N'Phẫu thuật bóc u mỡ/u bã đậu', N'Phẫu thuật', 1000000, 80, N'Tiểu phẫu bóc tách khối u lành tính', 1, GETDATE());
GO
