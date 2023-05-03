directory=$(pwd)
./bin/neo4j-admin unbind
mv ./data/databases/graph.db/ ./data/databases/_graph.db/
cd /home/downloads/
tar xvzf ARCHIVE_NAME 
#or unzip ARCHIVE_NAME
cd $directory
mv /home/downloads/graph.db ./data/databases/
systemctl start neo4j
tail -f ./logs/neo4j.log
