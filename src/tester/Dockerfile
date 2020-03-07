FROM python:3


COPY requirements.txt ./
COPY wait-for-it.sh wait-for-it.sh
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

ENTRYPOINT [ "./wait-for-it.sh", "webapplication:80", "-t", "0", "--", "python", "./test_api_with_sim.py" ]
