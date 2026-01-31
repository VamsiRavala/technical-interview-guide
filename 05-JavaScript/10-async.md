# Async:

jQuery.get("https://httv", function(response){
});

Callback Hell:
------------------
jQuery.get("https://httv", function(response){
  jQuery.get("https://httv", function(response){
  });
});

Promises --> capture the result 

axios.get("http://").then(function(rsponde)
{
 return axio.get()
 }). then func

 Async and Await:
 ---------------

 async await --> 

 var response1= await axios.get()

 
