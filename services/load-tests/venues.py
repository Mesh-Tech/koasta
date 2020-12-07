from locust import HttpLocust, TaskSet

def fetch_venues(l):
    l.client.get("/venue")

class UserBehavior(TaskSet):
    tasks = {fetch_venues: 1}

class WebsiteUser(HttpLocust):
    task_set = UserBehavior
    min_wait = 5000
    max_wait = 9000
