# Objects
- if you need complex than simplex data
- {} --> object (empty object)
- var nonemptyObject={ label:"myval"}

## manipulating objects:
<img width="996" height="441" alt="image" src="https://github.com/user-attachments/assets/43a9a096-b331-45cd-a971-6cc8b3a99cfe" />
<img width="1062" height="422" alt="image" src="https://github.com/user-attachments/assets/e4284dc6-281b-474a-93ff-1a5da236a524" />

## delete property from object:
delete bird.color

<img width="442" height="73" alt="image" src="https://github.com/user-attachments/assets/f4521419-aa61-434e-b2f2-304a26cc372b" />


## References:
  var animal={ genus:'corvus', species:'corvax', commonName:'raven'}

- Referncing the object especialy copy
- Not copy, two variable referring same copy

  ## if you want to copy? (... means Spread operator)
  - var animal={ genus:'corvus', species:'corvax', commonName:'raven'}
    - var animal2={ genus:'corvus2', species:'corvax2', commonName:'raven'}
    - var animal2=Object.assign({}, animal);
    - animal2={...animal}
    - animal2=JSON.parse(JSON.stringify(animal));
   
  - Copying done, you can change any way
 
    
 
    



