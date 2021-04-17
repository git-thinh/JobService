./redis-server.exe --service-install redis.windows-service.conf --loglevel verbose
sc.exe delete Redis


redis-server.exe --service-install --service-name redis5_1000 --port 1000 --dbfilename 1000.rdb --logfile 1000.log
sc.exe delete redis5_1000
redis-server --dbfilename dump.rdb --dir /home/user/dbs


redis-server.exe --port 6000 --dbfilename 6000.rdb --logfile 6000.log