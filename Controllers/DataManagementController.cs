using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Autodesk.Forge;
using Autodesk.Forge.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ForgeDerivative.Controllers
{
    public class DataManagementController : Controller
    {
        private Credentials Credentials { get; set; }

        [HttpGet]
        [Route("api/forge/datamanagement")]
        public async Task<IList<jsTreeNode>> GetTreeNodeAsync([FromQuery]string id)
        {
            Credentials = await Credentials.FromSessionAsync();
            if (Credentials == null)
            {
                return null;
            }
            IList<jsTreeNode> nodes = new List<jsTreeNode>();
            if (id == "#")
            {
                return await GetHubAsync();
            }
            else
            {
                string[] idParams = id.Split('/');
                string resource = idParams[idParams.Length - 2];
                switch (resource)
                {
                    case "hubs":
                        return await GetProjectsAsync(id);
                    case "projects":
                        return await GetProjectContents(id);
                    case "folders":
                        return await GetFolderCountents(id);
                    case "items":
                        return await GetItemVersions(id);
                }
            }
            return nodes;
        }

        private async Task<IList<jsTreeNode>> GetHubAsync()
        {
            IList<jsTreeNode> nodes = new List<jsTreeNode>();
            HubsApi hubsApi = new HubsApi();
            hubsApi.Configuration.AccessToken = Credentials.TokenInternal;
            var hubs = await hubsApi.GetHubsAsync();
            foreach (KeyValuePair<string, dynamic> hubInfo in new DynamicDictionaryItems(hubs.data))
            {
                string nodeType = "hubs";     
                switch ((string)hubInfo.Value.attributes.extension.type)
                {
                    case "hubs:autodesk.core:Hub":
                        nodeType = "hubs";
                        break;
                    case "hubs:autodesk.a360:PersonalHub":
                        nodeType = "personalHub";
                        break;
                    case "hubs:autodesk.bim360.Account":
                        nodeType = "bim360Hubs";
                        break;
                }
                jsTreeNode hubNode = new jsTreeNode(hubInfo.Value.links.self.href, hubInfo.Value.attributes.name,
                    nodeType, true);
                nodes.Add(hubNode);
            }
            return nodes;
        }

        private async Task<IList<jsTreeNode>> GetProjectsAsync(string href)
        {
            IList<jsTreeNode> nodes = new List<jsTreeNode>();
            ProjectsApi projectsApi = new ProjectsApi();
            projectsApi.Configuration.AccessToken = Credentials.TokenInternal;
            string[] idParams = href.Split('/');
            string hubId = idParams[idParams.Length - 1];
            var projects = await projectsApi.GetHubProjectsAsync(hubId);
            foreach (KeyValuePair<string, dynamic> projectInfo in new DynamicDictionaryItems(projects.data))
            {
                string nodeType = "projects";
                switch ((string)projectInfo.Value.attributes.extension.type)
                {
                    case "projects:autodesk.core:Project":
                        nodeType = "a360projects";
                        break;
                    case "projects:autodesk.bim360:Projects":
                        nodeType = "bim360projects";
                        break;
                }
                jsTreeNode projectNode = new jsTreeNode(projectInfo.Value.links.self.href, projectInfo.Value.attributes.name, nodeType, true);
                nodes.Add(projectNode);
            }
            return nodes;
        }


        private async Task<IList<jsTreeNode>> GetProjectContents(string href)
        {
            IList<jsTreeNode> nodes = new List<jsTreeNode>();
            ProjectsApi projectsApi = new ProjectsApi();
            projectsApi.Configuration.AccessToken = Credentials.TokenInternal;
            string[] idParams = href.Split('/');
            string hubId=idParams[idParams.Length-3];
            string projectId=idParams[idParams.Length-1];
            var project=await projectsApi.GetProjectAsync(hubId,projectId);
            var rootFolderHref=project.data.relationships.rootFolder.meta.link.href;
            return await GetFolderCountents(rootFolderHref);
        }

        private async Task<IList<jsTreeNode>> GetFolderCountents(string href)
        {
            IList<jsTreeNode> nodes=new List<jsTreeNode>();
            FoldersApi foldersApi=new FoldersApi();
            foldersApi.Configuration.AccessToken=Credentials.TokenInternal;
            string[] idParams=href.Split('/');
            string folderId=idParams[idParams.Length-1];
            string projectId=idParams[idParams.Length-3];
            var folderContents=await foldersApi.GetFolderContentsAsync(projectId,folderId);
            foreach(KeyValuePair<string,dynamic> folderContentItem in new DynamicDictionaryItems(folderContents.data))
            {
                string displayName=folderContentItem.Value.attributes.displayName;
                jsTreeNode itemNode=new jsTreeNode(folderContentItem.Value.links.self.href,displayName,(string)folderContentItem.Value.type,true);
                nodes.Add(itemNode);
            }
            return nodes;
        }

        private async Task<IList<jsTreeNode>> GetItemVersions(string href)
        {
            List<jsTreeNode> nodes=new List<jsTreeNode>();
            ItemsApi itemsApi=new ItemsApi();
            itemsApi.Configuration.AccessToken=Credentials.TokenInternal;
            string[] idParams=href.Split('/');
            string itemId=idParams[idParams.Length-1];
            string projectId=idParams[idParams.Length-3];
            var versions=await itemsApi.GetItemVersionsAsync(projectId,itemId);
            foreach(KeyValuePair<string,dynamic> version in new DynamicDictionaryItems(versions.data))
            {
                DateTime versionDate=version.Value.attributes.lastModifiedTime;
                string urn=string.Empty;
                try
                {
                    urn=(string)version.Value.relationships.derivatives.data.id;
                }
                catch
                {
                    urn="not_available";                    
                }
                jsTreeNode node=new jsTreeNode(urn,versionDate.ToString("dd/MM/yy HH:mm:ss"),"versions",false);
                nodes.Add(node);
            }
            return nodes;
        }
    }

    public class jsTreeNode
    {
        public string id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
        public bool children { get; set; }

        public jsTreeNode(string id, string text, string type, bool children)
        {
            this.id = id;
            this.text = text;
            this.type = type;
            this.children = children;
        }
    }
}