import sqlite3

def main():

    docString = "ITU-Minitwit Tweet Flagging Tool\n\n Usage:\n flag_tool <tweet_id>...\n flag_tool -i\n flag_tool -h\n Options:\n -h Show this screen.\n -i Dump all tweets and authors to STDOUT.\n";
    
    rc = sqlite3.connect("tmp/minitwit.db")
    if (rc):
        print("Can't open database")
    elif (len(sys.argv) == 2 and sys.argv[1] == "-h"):
        print(docString)
    elif (len(sys.argv) == 2 and sys.argv[1] == "-i"):
        cursor = rc.execute("SELECT * FROM messages")
        for row in cursor:
            print(row)
    elif ((len(sys.argv) >= 2 and sys.argv[1] != "-i") and sys.argv[1] != "-h"):
        for i in range(len(sys.argv)):
            rc.execute("UPDATE message SET flagged=1 WHERE message_id = %s", sys.argv[i])

if __name__ == "__main__":
    main()

