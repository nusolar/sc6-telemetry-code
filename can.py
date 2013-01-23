#!/usr/bin/env python

bases = (0x200, 0x210, 0x300, 0x310, 0x500, 0x400, 0x710, 0x770, 0x110, 0x500)
roots = ['_'.join([t,m,'']) for t in ('bms','sw','ws20','mppt','dc') for m in ('rx','tx')]
groups= (['trip','reset_cc_batt','reset_cc_array','reset_cc_mppt1',
		  'reset_cc_mppt2','reset_cc_mppt3', 'reset_wh','reset_all'],
		 ['heartbeat','error','uptime','last_reset',
		  'batt_bypass','current','cc_array','cc_batt',
		  'cc_mppt1','cc_mppt2','cc_mppt3','wh_batt',
		  'wh_mppt1','wh_mppt2','wh_mppt3','voltage',
		  'owvoltage','temp','trip','last_trip',
		  'trip_pt_current','trip_pt_voltage','trip_pt_temp'],
		 ['lights'],
		 ['heartbeat','error','buttons','lights'],
		 ['dc_id','drive_cmd','power_cmd','reset_cmd'],
		 ['motor_id','motor_status_info','motor_bus_msmt','motor_velocity_msmt',
		  'motor_phase_msmt','motor_vector_msmt','current_vector_msmt','backemf_msmt'
		  '15V_1PT65V_msmt','2PT5V_1PT2V_msmt','fanspeed_msmt','sink_motor_temp_msmt',
		  'airin_cpu_temp_msmt','airout_cap_temp_msmt','odom_bus_ah_msmt'],
		 ['mppt1','mppt2','mppt3'],
		 ['mppt1','mppt2','mppt3'],
		 ['horn','signals','cruise','cruise_velocity_current'],
		 ['drv_id'])
temp = [[(r+end,id) for end,id in zip(gs,range(n,n+len(gs))) ] for r,gs,n in zip(roots,groups,bases)]
idList = [canid for group in temp for canid in group]
addr = dict(idList)
name = {v:k for k,v in addr.items()}