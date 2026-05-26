import re

def extract_and_append():
    with open('ef_schema.sql', 'r', encoding='utf-8') as f:
        content = f.read()

    with open('BenhVien.sql', 'r', encoding='utf-8') as f:
        benhvien_content = f.read()
        
    tables_to_add = ['BloodBanks', 'MedicalEquipments', 'MedicalServices', 'QualityReviews', 'DiagnosticImages', 'InsuranceCards', 'Receptions', 'Surgeries']
    
    append_str = "\n\n-- Bảng bổ sung từ Model\n"
    for table in tables_to_add:
        if f"CREATE TABLE [{table}]" not in benhvien_content and f"CREATE TABLE {table}" not in benhvien_content:
            # Extract CREATE TABLE block from ef_schema.sql
            pattern = r"CREATE TABLE \[" + table + r"\] \([\s\S]*?\);\nGO\n"
            match = re.search(pattern, content)
            if match:
                append_str += match.group(0) + "\n"
            else:
                # Sometimes GO is not there or the regex misses
                pattern2 = r"CREATE TABLE \[" + table + r"\] \([\s\S]*?\);"
                match2 = re.search(pattern2, content)
                if match2:
                    append_str += match2.group(0) + "\nGO\n\n"
                    
    with open('BenhVien.sql', 'a', encoding='utf-8') as f:
        f.write(append_str)

extract_and_append()
