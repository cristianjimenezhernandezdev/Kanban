using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WpfAppTestAPIClient.Model;
using System.Configuration;

namespace WpfAppTestAPIClient.APIClient
{
    public class UsersApiClient
    {
        string BaseUri;
        
        
        public UsersApiClient()
        {
            BaseUri = ConfigurationManager.AppSettings["BaseUri"];
        }
        
        /// <summary>
        /// Obté un usuari a partir del Id
        /// </summary>
        /// <param name="Id">Codi d'usuari</param>
        /// <returns>Usuari o null si no el troba</returns>
        public async Task<User> GetUserAsync(int Id)
        {
            User user = new User();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Enviem una petició GET al endpoint /users/{Id}
                HttpResponseMessage response = await client.GetAsync($"users/{Id}");
                if (response.IsSuccessStatusCode)
                {
                    //Reposta 204 quan no ha trobat dades
                    if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    {
                        user = null;
                    }
                    else
                    {
                        //Obtenim el resultat i el carreguem al Objecte User
                        user = await response.Content.ReadAsAsync<User>();
                        response.Dispose();
                    }
                }
                else
                {
                    //TODO: que fer si ha anat malament? retornar null? 
                }
            }
            return user;
        }

        /// <summary>
        /// Obté una llista de tots els usuaris de la base de dades
        /// </summary>
        /// <returns></returns>
        public async Task<List<User>> GetUsersAsync()
        {
            List<User> users = new List<User>();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Enviem una petició GET al endpoint /users}
                HttpResponseMessage response = await client.GetAsync("users");
                if (response.IsSuccessStatusCode)
                {
                    //Obtenim el resultat i el carreguem al objecte llista d'usuaris
                    users = await response.Content.ReadAsAsync<List<User>>();
                    response.Dispose();
                }
                else
                {
                    //TODO: que fer si ha anat malament? retornar null? missatge?
                }
            }
            return users;
        }

        /// <summary>
        /// Afegeix un nou usuari
        /// </summary>
        /// <param name="user">Usuari que volem afegir</param>
        /// <returns></returns>
        public async Task AddAsync(User user)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Enviem una petició POST al endpoint /users}
                HttpResponseMessage response = await client.PostAsJsonAsync("users", user);
                response.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Modificar un usuari
        /// </summary>
        /// <param name="user">Usuari que volem modificar</param>
        /// <returns></returns>
        public async Task UpdateAsync(User user)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Enviem una petició PUT al endpoint /users/Id
                HttpResponseMessage response = await client.PutAsJsonAsync($"users/{user.Id}", user);
                response.EnsureSuccessStatusCode();
            }
        }


        /// <summary>
        /// Modificar un usuari
        /// </summary>
        /// <param name="user">Usuari que volem modificar</param>
        /// <returns></returns>
        public async Task DeleteAsync(int Id)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Enviem una petició DELETE al endpoint /users/Id
                HttpResponseMessage response = await client.DeleteAsync($"users/{Id}");
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
