using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MessagePack.Internal;
using System.Linq.Expressions;
using System.Net;

namespace TechTestBackend.Controllers;

[ApiController]
[Route("api/spotify")]
public class SpotifyController : ControllerBase
{
    //constructors are obsolete should not be used anymore
    public SpotifyController()
    {

    }

    [HttpGet]
    [Route("searchTracks")]
    public IActionResult SearchTracks(string name)
    {
        try
        {        
            // TODO: Implement this method
            var track = SpotifyHelper.GetTracks(name);

            return Ok(track);
        }
        catch (Exception e)
        {
            // this is the best practice for not leaking error details
            
            return NotFound(new {message = e.Message});
        }
    }

    [HttpPost]
    [Route("like")]
    public IActionResult Like(string id)
    {
        var storage = HttpContext.RequestServices.GetService(typeof(SongstorageContext));
        
        var track = SpotifyHelper.GetTrack(id); //check if trak exists
        if(track.Id == null || SpotifyId(id) == false)
        {
            return NotFound();
        }

        var song = new Soptifysong(); //create new song
        song.Id = id;
        song.Name = track.Name;

        try
        {
            //crashes sometimes for some reason
            // we   have to look into this
            ((SongstorageContext)storage).Songs.Add(song);

            ((SongstorageContext)storage).SaveChanges();
        }
        catch (Exception e)
        {
            // not sure if this is the best way to handle this
            return StatusCode(500, new {message = e.Message });
        }
        
        return Ok();
    }
    
    [HttpPost]
    [Route("removeLike")]
    public IActionResult RemoveLike(string id)
    {
        var storage = HttpContext.RequestServices.GetService(typeof(SongstorageContext));
        
        var track = SpotifyHelper.GetTrack(id);
        if(track.Id == null || SpotifyId(id) == false)
        {
            return NotFound(); // bad request wrong id not existing in spotify
        }

        var song = new Soptifysong();
        song.Id = id;
        
        try
        {
            ((SongstorageContext)storage).Songs.Remove(song); // this is not working every tume
            ((SongstorageContext)storage).SaveChanges();
        }
        catch (Exception e)
        {
            // we should probably log this
            return StatusCode(500, new { message = e.Message } );
        }
        
        return Ok();
    }
    
    [HttpGet]
    [Route("listLiked")]
    public IActionResult ListLiked()
    {
        var storage = HttpContext.RequestServices.GetService(typeof(SongstorageContext));

        int songsnumber = ((SongstorageContext)storage).Songs.Count();
        List<Soptifysong> songs = new List<Soptifysong>(); //((SongstorageContext)storage).Songs.ToList();

        if (songsnumber > 0)
        {
            for (int i = 0; i <= songsnumber - 1; i++)
            {
                var spotifySong = ((SongstorageContext)HttpContext.RequestServices.GetService(typeof(SongstorageContext))).Songs.ToList()[i];

                var track = SpotifyHelper.GetTrack(spotifySong.Id);
                if (track.Id == null)
                {
                    // TODO: remove song from database, but not sure how
                    ((SongstorageContext)storage).Songs.Remove(spotifySong);
                    ((SongstorageContext)storage).SaveChanges();

                }
                else
                {
                    // not working for some reason so we have to do the check manually for now
                    // if(SongExists(track.Id) == false)

                    songs.Add(spotifySong);

                }

            }
        }

        //save the changes, just in case

        return Ok(songs);
    }
    
    private bool SongExists(string id)
    {
        return ((SongstorageContext)HttpContext.RequestServices.GetService(typeof(SongstorageContext))).Songs.First(e => e.Id == id) != null;
    }
    
    private static bool SpotifyId(object id)
    {
        return id.ToString().Length == 22;
    }
}