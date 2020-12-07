#!/usr/bin/env python3

import json
import os
import sys
import subprocess
import shutil
import tempfile
import time

from distutils.version import LooseVersion

working_dir = os.path.dirname(os.path.realpath(__file__))
rootpath = os.path.abspath(working_dir + "/../")
dbpath = os.path.abspath(os.path.join(rootpath, "database", "migrations"))
environment = sys.argv[1]
kubeconfig = ''
env = os.environ.copy()

config = json.load(open(working_dir + '/../services/Auth/config/' + environment + '.json'))
db_conn_string = config['connection']['database_url']
components = db_conn_string.split(";")

db = [item for item in components if item.startswith('Database')][0].split('=')[1]
username = [item for item in components if item.startswith('User ID')][0].split('=')[1]
password = [item for item in components if item.startswith('Password')][0].split('=')[1]
host = [item for item in components if item.startswith('Host')][0].split('=')[1]

db_url = "jdbc:postgresql://" + host + "/" + db

process = subprocess.Popen("docker run --rm -v "+dbpath+":/flyway/sql flyway/flyway -url="+db_url+" -user="+username+" -password="+password+" migrate", shell=True, stdout=subprocess.PIPE, cwd=working_dir, env=env)
while True:
    nextline = process.stdout.readline()
    if nextline == b'' and process.poll() is not None:
        break
    sys.stdout.write(nextline)
    sys.stdout.flush()

exitCode = process.returncode

if (exitCode != 0):
    raise Exception('Migration failed')

finished = False
failed = False

print("Done")
