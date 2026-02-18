### 9. Assume your program will be used on much larger data files. Describe in a few sentences what you would change if you knew it would be used for a 10GB CSV input file.

    
Since I already use StreamReader and SqlBulkCopy, the application handles memory efficiently. 
However, with a 10GB file, the in-memory HashSet for finding duplicates would crash the program due to a lack of RAM. To solve this, I would move the duplicate check to the database side. 
I would load all raw data into a temporary Staging Table first, and then use a SQL MERGE command to copy only the unique records into the main table.
I would also disable indexes before the upload to make it faster and enable them again once finished.
