#!/usr/bin/env python

import pika, db, thread

halt = False
con = db.con()

def receive():
    connection = pika.BlockingConnection(pika.ConnectionParameters(host='mbr.chandel.net'))
    channel = connection.channel()
    channel.queue_declare(queue='telemetry')
    def quit():
        channel.close(); connection.close(); thread.exit()
    def callback(ch, method, properties, pkt):
        t = pkt.find('t')
        v = (float(pkt[0:t]),int('0x'+pkt[t+1:t+4],16),int('0x'+pkt[t+5:],16))
        print v
        con.execute("INSERT INTO packets VALUES (?,?,?)", v)
        con.commit()
        if halt: quit()
    channel.basic_consume(callback,queue='telemetry',no_ack=True)
    try: channel.start_consuming()
    except: quit()