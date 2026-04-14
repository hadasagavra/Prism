using AutoMapper;
using Common.Dto;
using Common.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Repository.Entities;
using Repository.Interfaces;
using Service.Interfaces;

[ApiController]
[Route("api/[controller]")]
//[Authorize]
public class EmailRequestsController : ControllerBase
{
    private readonly IEmail _service;
    private readonly IMapper _mapper;

    public EmailRequestsController(IEmail service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<EmailRequest>>> GetAll()
    {
        var dtos = await _service.GetAll();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmailRequest>> GetById(int id)
    {
        var dto = await _service.GetById(id);
        dto.ThrowIfNotFound();
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<EmailRequest>> Create([FromBody] CreateEmailRequestDto dto)
    {
      
        var created = await _service.AddItem(dto);
        var result = _mapper.Map<EmailRequestDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEmailRequestDto dto)
    {
        var updated = await _service.UpdateItem(id, (dto));
          return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        await _service.DeleteItem(id);
        return NoContent();
    }

    [HttpPatch("{id}/classify")]
    //[Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmailRequest>> Classify(int id, [FromBody] ClassifyEmailDto dto)
    {
        var updated = await _service.UpdateClassification(id,dto);
        return Ok(updated);
    }
}
