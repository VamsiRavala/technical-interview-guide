# GraphQl
## Introduction:
  - Facebook, friends or --> 4 or five level filter
  - Query Language
  - Ask what you want.
  - Type Checking

## Eco System:
 - Need

query {
  getProduct(id:"f543f9a284603ba2c7fe"){
    price
    name
    description
    stores {
      store
    }
  }
}
<img width="1920" height="1020" alt="image" src="https://github.com/user-attachments/assets/5df9b731-ceab-4860-9b85-7ad88f738f76" />


mutation{
  createProduct(input:{
    name:"Widget2",
    description:"another widget2",
    price:33.99,
    soldout: false,
    stores: [
      {store: "NY"}, {store:"CA"}
    ]
  }) {
    price
    name
    description
    id
  }
}

<img width="1920" height="1020" alt="image" src="https://github.com/user-attachments/assets/3b58be23-88ab-4f98-895a-46e4bd22f115" />
