# DataCenter:
 - Physical storage 
# Region:
- One or more Azure Data Center is grouped is Region

# Availablity Zones
 - One or more Azure Data Center is grouped is Region
 - Many Azure regions provide availability zones.
 - Availability zones are independent sets of datacenters that contain isolated power, cooling, and network connections.
 - Availability zones are physically located close enough together to provide a low-latency network, but far enough apart to provide fault isolation from such things as storms and isolated power outages

<img width="962" height="373" alt="image" src="https://github.com/user-attachments/assets/95f2194e-58ff-4b97-9344-34b2f1e740dc" /> 

# Azure Availablity Types


| **Redundancy Type** | **Zones Used** | **Protection Scope** |
|----------------------|----------------|-----------------------|
| **LRS** | 1 zone | Datacenter failure risk |
| **ZRS** | 3 zones in same region | Zone-level protection |
| **GZRS** | 3 zones + secondary region | Region-level protection |
| **RA-GZRS** | Same as GZRS + read access to secondary | Maximum durability & availability |







