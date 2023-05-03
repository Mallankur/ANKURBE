from neo4j import GraphDatabase, Transaction
import json
import http.client
import os

with open("config.json", "r") as config_json:
    configuration = json.loads(config_json.read())

uri = configuration["neo4jHost"]
driver = GraphDatabase.driver(uri,
                              auth=(configuration["neo4jUsername"],
                                    configuration["neo4jPassword"]))
query = "MATCH (t:Tenant)-[:BELONGS]-(g:Group)-[:MEMBER_OF]-(s:Subject),(g)-[:ASSIGNED]-(r:Role)-[:CONTAINS]-(p:Policy) RETURN t.Id as `tenantId`,s.Id as `subjectId` LIMIT 10000"
data_file_name = "data.json"
token_file_name = "token.txt"


def get_subject_ids(tx: Transaction):
    result = map(lambda r: dict(r), tx.run(query))
    return list(result)


def get_token() -> str:
    conn = http.client.HTTPSConnection(configuration["oauthHost"])
    payload = f'grant_type=client_credentials&scope=https%3A//api.adform.com/scope/bloom.runtime&client_id={configuration["oauthClientId"]}&client_secret={configuration["oauthClientSecret"]}'
    headers = {'Content-Type': 'application/x-www-form-urlencoded'}
    conn.request("POST", "/sts/connect/token", payload, headers)
    res = conn.getresponse()

    if res.status >= 400:
        raise Exception("Failed to retrieve token. Exiting.")

    data = res.read()
    content = json.loads(data.decode("UTF8"))
    return content["access_token"]


def clean_up():
    if os.path.exists(data_file_name):
        os.remove(data_file_name)
    if os.path.exists(token_file_name):
        os.remove(token_file_name)


def main():
    clean_up()
    token = get_token()

    with open(token_file_name, "w") as file:
        file.write(token)

    db_records = []
    with driver.session() as session:
        db_records = session.read_transaction(get_subject_ids)

    with open(data_file_name, "w") as file:
        file.writelines(json.dumps(db_records))


if __name__ == '__main__':
    main()
