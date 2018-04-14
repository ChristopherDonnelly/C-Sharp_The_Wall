using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using The_Wall.Models;
using DbConnection;
using Login_Register.Models;
using Microsoft.AspNetCore.Http;

namespace The_Wall.Controllers
{
    public class WallController : Controller
    {
        private readonly DbConnector _dbConnector;
        private User currentUser;

        public WallController(DbConnector connect)
        {
            _dbConnector = connect;
        }

        [HttpGet]
        [Route("")]
        [Route("Wall")]
        public IActionResult Wall()
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            string UserName = HttpContext.Session.GetString("UserName");
            
            ViewData["Username"] = UserName;

            if(UserId!=null){

                ViewData["UserId"] = (int)UserId;

                List<Dictionary<string, object>> user = _dbConnector.Query($"SELECT id, first_name, last_name FROM users WHERE id='{UserId}'");

                currentUser = new User
                {
                    UserId = (int)user[0]["id"],
                    FirstName = (string)user[0]["first_name"],
                    LastName = (string)user[0]["last_name"]
                };

                ModelBundle ViewBundle = new ModelBundle{ 
                    AllMessages = getAllMessages(), 
                    SingleMessage = new Message{
                        UserId = currentUser.UserId
                    }
                };

                return View(ViewBundle);
            }else{
                return RedirectToAction("Login", "User");
            }
        }

        [HttpPost]
        [Route("PostMessage")]
        public IActionResult PostMessage(Message model)
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            string UserName = HttpContext.Session.GetString("UserName");
            
            ViewData["UserId"] = (int)UserId;
            ViewData["Username"] = UserName;

            if(ModelState.IsValid)
            {

                Console.WriteLine("**********************************");
                Console.WriteLine(model.UserId);
                Console.WriteLine(model.MessageContent);
                Console.WriteLine("**********************************");

                string insertQuery = $"INSERT INTO messages (user_id, message, created_at, updated_at) VALUES ('{model.UserId}', '{model.MessageContent}', NOW(), NOW())";

                _dbConnector.Execute(insertQuery);

                RedirectToAction("Wall");
            }

            model.MessageContent = "";

            ModelBundle ViewBundle = new ModelBundle{ 
                AllMessages = getAllMessages(), 
                SingleMessage = new Message{
                    UserId = (int)UserId,
                    MessageContent = ""
                }
            };

            return View("Wall", ViewBundle);
        }

        [HttpPost]
        [Route("PostComment")]
        public IActionResult PostComment(Comment model)
        {
            int? UserId = HttpContext.Session.GetInt32("UserId");
            string UserName = HttpContext.Session.GetString("UserName");
            
            ViewData["UserId"] = (int)UserId;
            ViewData["Username"] = UserName;

            if(ModelState.IsValid)
            {

                Console.WriteLine("**********************************");
                Console.WriteLine(model.UserId);
                Console.WriteLine(model.MessageId);
                Console.WriteLine(model.Content);
                Console.WriteLine("**********************************");

                string insertQuery = $"INSERT INTO comments (message_id, user_id, comment, created_at, updated_at) VALUES ('{model.MessageId}', '{model.UserId}', '{model.Content}', NOW(), NOW())";

                _dbConnector.Execute(insertQuery);

                return RedirectToAction("Wall");
            }

            ViewData["_"+model.MessageId] = "Comment is required!";
            model.Content = "";

            ModelBundle ViewBundle = new ModelBundle{ 
                AllMessages = getAllMessages(), 
                SingleMessage = new Message{
                    UserId = (int)UserId,
                    MessageContent = ""
                }
            };

            return View("Wall", ViewBundle);
        }

        [HttpPost]
        [Route("DeleteMessage")]
        public IActionResult DeleteMessage(Message message)
        {
            Console.WriteLine("**********************************");
            Console.WriteLine($"*    Deleting Message Id: {message.MessageId}");
            Console.WriteLine("**********************************");

            string deleteQuery = $"DELETE FROM messages WHERE id={message.MessageId}";
            _dbConnector.Execute(deleteQuery);

            return RedirectToAction("Wall");
        }

        [HttpPost]
        [Route("DeleteComment")]
        public IActionResult DeleteComment(Comment comment)
        {
            Console.WriteLine("**********************************");
            Console.WriteLine($"*    Deleting Comment Id: {comment.CommentId}");
            Console.WriteLine("**********************************");

            string deleteQuery = $"DELETE FROM comments WHERE id={comment.CommentId}";
            _dbConnector.Execute(deleteQuery);

            return RedirectToAction("Wall");
        }

        public List<Message> getAllMessages(){

            List<Message> AllMessages = new List<Message>();

            string msgQuery = "SELECT users.id, concat(users.first_name, ' ', users.last_name) as full_name, messages.id as message_id, messages.user_id, messages.message, messages.created_at, DATE_FORMAT(messages.created_at, '%M %D %Y') as date FROM messages JOIN users on messages.user_id = users.id ORDER BY messages.created_at DESC";
            List<Dictionary<string, object>> messages = _dbConnector.Query(msgQuery);
            
            string commentQuery = "SELECT users.id, concat(users.first_name, ' ', users.last_name) as full_name, comments.id as comment_id, comments.message_id as message_id, comments.comment, comments.created_at, DATE_FORMAT(comments.created_at, '%M %D %Y') as date FROM comments LEFT JOIN users on comments.user_id = users.id ORDER BY comments.created_at ASC";
            List<Dictionary<string, object>> comments = _dbConnector.Query(commentQuery);

            foreach(Dictionary<string, object> message in messages){
                int messageId = (int)message["message_id"];
                List<Comment> messageComments = new List<Comment>();
                foreach(Dictionary<string, object> comment in comments){
                    if(messageId == (int)comment["message_id"]){
                        messageComments.Add(new Comment {
                            UserId = (int)comment["id"],
                            FullName = (string)comment["full_name"],
                            MessageId = (int)comment["message_id"],
                            CommentId = (int)comment["comment_id"],
                            Content = (string)comment["comment"],
                            CreatedAt = (DateTime)comment["created_at"]
                        });
                    }
                }
                AllMessages.Add(new Message {
                    UserId = (int)message["id"],
                    FullName = (string)message["full_name"],
                    MessageId = messageId,
                    MessageContent = (string)message["message"],
                    Comments = messageComments,
                    CreatedAt = (DateTime)message["created_at"]
                });
            }

            // for message in messages:
            //      dt = datetime.datetime.now()-message['created_at']
            //      mins = divmod(dt.days * 86400 + dt.seconds, 60)[0]
            //      message['mins'] = mins

            // List<Dictionary<string, object>> AllMessages = _dbConnector.Query("SELECT id, first_name, last_name, email FROM messages");

            return AllMessages;
        }

    }
}
