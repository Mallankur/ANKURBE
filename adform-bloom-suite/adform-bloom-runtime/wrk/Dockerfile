FROM rajneeshmitharwal/docker-wrk2-json as base
WORKDIR /wrk

RUN apt update
# RUN apt install -y build-essential zlib1g-dev libncurses5-dev libgdbm-dev libnss3-dev libssl-dev libreadline-dev libffi-dev wget

# WORKDIR /tmp
# RUN curl -O https://www.python.org/ftp/python/3.7.3/Python-3.7.3.tar.xz
# RUN tar -xf Python-3.7.3.tar.xz
# RUN ./Python-3.7.3/configure --enable-optimizations
# RUN ls ./Python-3.7.3
# RUN ./Python-3.7.3/make -j 8
# RUN ./Python-3.7.3/make altinstall

### why it installs python 3.4?
RUN apt install python3
RUN python3 --version

RUN apt install python3-pip -y

RUN pip3 install --upgrade pip

### cant install dependencies
# RUN pip3 install --upgrade setuptools
# COPY requirements.txt .
# RUN pip3 install -r requirements.txt --upgrade pip

CMD ["/bin/sh"]
