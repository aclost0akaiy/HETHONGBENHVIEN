import os
import codecs

def convert_to_utf8_bom(directory):
    converted_count = 0
    warning_count = 0
    
    for root, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith('.cshtml'):
                filepath = os.path.join(root, file)
                try:
                    # Read the raw bytes first to check if BOM is already present
                    with open(filepath, 'rb') as f:
                        raw = f.read()
                    
                    if raw.startswith(codecs.BOM_UTF8):
                        # Already has UTF-8 BOM, skip
                        continue
                        
                    # Try to decode as UTF-8
                    try:
                        content = raw.decode('utf-8')
                        encoding_used = 'utf-8'
                    except UnicodeDecodeError:
                        # If UTF-8 fails, try windows-1258 (Vietnamese ANSI) or utf-16
                        try:
                            content = raw.decode('windows-1258')
                            encoding_used = 'windows-1258'
                            print(f"Warning: {filepath} was decoded using windows-1258")
                        except UnicodeDecodeError:
                            content = raw.decode('utf-16')
                            encoding_used = 'utf-16'
                            print(f"Warning: {filepath} was decoded using utf-16")
                    
                    # Write back with UTF-8 BOM
                    with open(filepath, 'w', encoding='utf-8-sig') as f:
                        f.write(content)
                    
                    print(f"Successfully converted: {filepath} (Original encoding: {encoding_used})")
                    converted_count += 1
                except Exception as e:
                    print(f"Error processing {filepath}: {e}")
                    warning_count += 1
                    
    print(f"\nFinished. Converted {converted_count} files. Errors/Warnings: {warning_count}")

if __name__ == '__main__':
    # Target directory is the Views folder relative to the script location
    views_dir = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', 'Views'))
    print(f"Scanning directory: {views_dir}")
    convert_to_utf8_bom(views_dir)
