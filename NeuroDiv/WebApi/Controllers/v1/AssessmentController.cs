using Application.Features.Assessment.Command;
using Application.Features.Assessment.Query;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class AssessmentssssController : BaseApiController
    {
        /// <summary>
        /// Get all assessment templates visible to the current therapist.
        /// Includes system templates + org-wide templates + own private templates.
        /// </summary>
        /// <param name="organizationId"></param>
        /// <param name="isSystemTemplate"></param>
        /// <param name="isActive"></param>
        /// <param name="targetCondition"></param>
        /// <returns></returns>
        [HttpGet("templates")]
        public async Task<IActionResult> GetTemplates([FromQuery] Guid? organizationId, [FromQuery] bool? isSystemTemplate,
                                                    [FromQuery] bool? isActive, [FromQuery] string? targetCondition)
        {
            return Ok(await Mediator.Send(new GetAssessmentTemplatesQuery
            {
                OrganizationId = organizationId,
                IsSystemTemplate = isSystemTemplate,
                IsActive = isActive,
                TargetCondition = targetCondition,
            }));
        }

        /// <summary>
        /// Get a specific template with all sections, questions and options.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("templates/{id}")]
        public async Task<IActionResult> GetTemplate(Guid id)
        {
            return Ok(await Mediator.Send(new GetAssessmentTemplateQuery { Id = id }));
        }

        /// <summary>
        /// Create a custom assessment template.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("templates")]
        public async Task<IActionResult> CreateTemplate(CreateAssessmentTemplateCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Update a custom template — name, description, visibility, active status.
        /// System templates cannot be modified.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPut("templates/{id}")]
        public async Task<IActionResult> UpdateTemplate(Guid id, UpdateAssessmentTemplateCommand command)
        {
            command.Id = id;
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Get all assessments for a specific child — most recent first.
        /// Filterable by template and date range.
        /// </summary>
        /// <param name="childProfileId"></param>
        /// <param name="templateId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        [HttpGet("children/{childProfileId}/snapshots")]
        public async Task<IActionResult> GetSnapshots(Guid childProfileId, [FromQuery] Guid? templateId,
                                                      [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            return Ok(await Mediator.Send(new GetAssessmentSnapshotsQuery
            {
                ChildProfileId = childProfileId,
                TemplateId = templateId,
                FromDate = fromDate,
                ToDate = toDate,
            }));
        }

        /// <summary>
        /// Get a specific assessment snapshot with all responses.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("snapshots/{id}")]
        public async Task<IActionResult> GetSnapshot(Guid id)
        {
            return Ok(await Mediator.Send(new GetAssessmentSnapshotQuery { Id = id }));
        }

        /// <summary>
        /// Submit a completed digital assessment.
        /// Creates an immutable snapshot with auto-calculated scores.
        /// Can be standalone or linked to a session occurrence.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("snapshots/submit")]
        public async Task<IActionResult> Submit(SubmitAssessmentCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Upload a legacy (paper) assessment.
        /// Creates a pending snapshot awaiting AI extraction and human verification.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost("snapshots/upload-legacy")]
        public async Task<IActionResult> UploadLegacy(UploadLegacyAssessmentCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
    }

}
