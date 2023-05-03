# Database 

Currently Bloom uses OngDb, which is equiparable to its counterpart Neo4j till version 3.x. Neo4j changed paths on its architecture of casual clustering on version 4.0. 

## Basic Setup

1. In VMaaS setup an instance, ideally flavor `c4.m16.50g` on `centos-7.6-64-base_2019-06-21_710` image should fine for CORE, REPLICA can have lower specs.

**Note 1:**
Is recommened to use low latency disk. To mount a disk for the first use commands. Also see: [Add volumne to the machine](https://adform.atlassian.net/wiki/spaces/PTC/pages/77529953/Add+volume+to+the+machine).

```bash
fdisk /dev/vdb
#Once requested for command, enter
n
#For the rest take default option. once asked for Command second time enter
w
mkfs.xfs /dev/vdb1
mount /dev/vdb1 /ongdb

## Add on /etc/fstab
##OngDB
/dev/vdb                              /ongdb           xfs     defaults 0 0
```
**Note 2:**
Open the following ports: 7474 (browser), 7687 (bolt)

2. Install Java

```bash
yum install java-1.8.0-openjdk
```

3. Get the latest release and decompress
```bash
wget -c https://cdn.graphfoundation.org/ongdb/dist/ongdb-enterprise-3.5.14-unix.tar.gz -O - | tar -xz
```
4. Setup systemd by adding the file called `ongdb.service` in `etc/systemd/system` (bear in mind that paths might differ) wiht the following content:

```bash
[Unit]
Description=OngDb
Documentation=https://neo4j.com/docs/operations-manual/3.5
After=network.target

[Service]
Type=forking
ExecStart=/ongdb/ongdb-enterprise-3.5.14/bin/neo4j start
ExecStop=/ongdb/ongdb-enterprise-3.5.14/bin/neo4j stop
ExecReload=/ongdb/ongdb-enterprise-3.5.14/bin/neo4j restart
TimeoutSec=120
KillMode=process
Restart=always
RestartSec=10
RuntimeDirectory=neo4j
LimitMEMLOCK=infinity
LimitNOFILE=60000
SecureBits=keep-caps
NoNewPrivileges=yes

[Install]
WantedBy=multi-user.target
```
5. To expose the node we need to adjust the following line in `conf/neo4j.conf`
:
```bash
dbms.connectors.default_listen_address=0.0.0.0
```

Additionally, if you want to expose metrics add the following lines at the end of the configuration file:


```bash
# metrics
metrics.prometheus.enabled=true
metrics.prometheus.endpoint=localhost:9187
```

6. After that we can start the service

```bash 
sudo systemctl start ongdb
```
**NOTE**
It is also good at this point to set the default password for the `neo4j` user. For that purpose `neo4j-admin set-initial-password <password>` command will set the password. Please remember also to update Nabu.

## Tuning

There are several optimization improvements:

1. Heap Sizing 

This is configured via `/conf/neo4j.conf` the parameters `dbms.memory.heap.initial_size` and `dbms.memory.heap.max_size` this two parameters should match. 

2. Number of Open Files

This is configured via the `neo4j.service` file. 

3. Page Cache Sizing

This is configured via `/conf/neo4j.conf` the parameter `dbms.memory.pagecache.size`

4. Transaction logs

This is configured via `/conf/neo4j.conf` the parameter `dbms.tx_log.rotation.retention_policy`

5. To experiment with, additional java tuning in `/conf/neo4j.conf`

`dbms.jvm.additional=-XX:MaxDirectMemorySize=6g`

`dbms.jvm.additional=-Dio.netty.maxDirectMemory=0`

6. The rule of thumb for estimating the node size `Total Physical Memory = Heap + Page Cache + OS Memory`

7. Go to `http://SERVER_ADDRESS:7474/browser/`, login with a default user and password (neo4j/neo4j) and set a new password.

## Database backup

To copy a database from one node to another it is needed to copy the following file `data\databases\graph.db` however this one might be lock by the service, therefore for all purposes is better to shutdown the node before. 

**Note** Before any node on the casual clustering is added, the node itself must contain the same data as other in the cluster, else it will fail in the catch up and it won't be enabled.

**Warning** When manipulating one node of the cluster (shutdown for a db backup) is important to run `neo4j-admin unbind` after the node is down as this will avoid issues when the node rejoins an active cluster

## Plugin install

* Download the plugin `*.jar` from http://github.com/neo4j-contrib/neo4j-apoc-procedures/releases/latest 
* Shutdown the service. 
* Copy the `jar` to the `/plugins` folder. 
* Allow the plugin to run by modifying the configuration `dbms.security.procedures.unrestricted=apoc.*`
* Restart the service.
* Verify that the procedure was installed by `CALL dbms.procedures()`

## Casual Clustering

OngDb uses the raft protocol for casual clustering. In this kind of approach 2 modes exist for the nodes CORE and READ_REPLICA.

### Core Setup

The CORE nodes are resposible for write operations and routing information. The minimum amount of nodes required for a cluster of CORE is 3, this is because CORE nodes need to find a LEADER through the quorum among the cluster members. Each CORE node in the cluster must have the list of all the other CORE members. We need to adjust `conf/neo4j.conf` in the following lines:

```bash
dbms.default_listen_address=0.0.0.0
dbms.default_advertised_address=core01.adform.com #can be ip address
dbms.mode=CORE
causal_clustering.initial_discovery_members=core01.adform.com:5000,core02.adform.com:5000,core03.adform.com:5000
```


### Replica Setup

The READ_REPLICA nodes are used for scaling horizontally. We need to adjust `conf/neo4j.conf` in the following lines:

```bash
dbms.mode=READ_REPLICA
dbms.default_advertised_address=replica01.adform.com #can be ip address
causal_clustering.initial_discovery_members=core01.adform.com:5000,core02.adform.com:5000,core03.adform.com:5000
```
**Note** 

It can be noted that the READ_REPLICA are not part of the `initial_discovery_members` this is due the fact that CORE member are routing the `default_advertised_address` of the READ_REPLICA as soon as this one joins the cluster.

## Monitoring

OngDb supports Prometheus by default. We need to add in `conf/neo4j.conf` the following lines: 

```bash 
# metrics
metrics.prometheus.enabled=true
metrics.prometheus.endpoint=localhost:9187
```

In order to fully integrate OngDB with Adform monitoring a few additional steps are needed:

1. You also need to make some changes in [puppet-control repository](https://gitz.adform.com/it/puppet-control/tree/main/hieradata/tiers). For example if your virtual machine name starts with `ongcia` you should add a file called `ongcia.json` in the following path `puppet-control\hieradata\tiers\ENV_NAME\host_group` and with the content as below. To check if the configuration was applied go to `http://SERVER_ADDRESS:9004/` and check if you can see there an exporter like `
application_9187_metrics`.

```bash
{
    "profiles::prometheus::exporters::config": {
      "application": {
        "enabled": true,
        "port": [ "9187" ]
      }
    }
}
```

2. To configure availability monitoring you need to modify [MAAS/targets](https://gitz.adform.com/MaaS/targets). For example see [here](https://gitz.adform.com/MaaS/targets/tree/master/tenants/Scope%20CIAM).
