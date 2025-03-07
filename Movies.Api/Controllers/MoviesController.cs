using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController(IMovieRepository movieRepository) : ControllerBase
{
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await movieRepository.GetAllAsync();

        var moviesResponse = movies.MapToMoviesResponse();

        return Ok(moviesResponse);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get(string idOrSlug)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) ? 
            await movieRepository.GetByIdAsync(id) :
            await movieRepository.GetBySlugAsync(idOrSlug);

        if (movie is null)
        {
            return NotFound();
        }
        
        var movieResponse = movie.MapToMovieResponse();

        return Ok(movieResponse);
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create(CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        
        var createdSuccessfully = await movieRepository.CreateAsync(movie);

        if (!createdSuccessfully)
        {
            return BadRequest();
        }
        
        var movieResponse = movie.MapToMovieResponse();

        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movieResponse);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateMovieRequest request)
    {
        var updatedMovie = request.MapToMovie(id);

        var updatedSuccessfully = await movieRepository.UpdateAsync(updatedMovie);

        if (!updatedSuccessfully)
        {
            return NotFound();
        }

        return Ok(updatedMovie);
    }

    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deletedSuccessfully = await movieRepository.DeleteByIdAsync(id);

        if (!deletedSuccessfully)
        {
            return NotFound();
        }

        return Ok();
    }
}