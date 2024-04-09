/********************************************************************************
 * Copyright (c) 2024 BMW Group AG
 *
 * See the NOTICE file(s) distributed with this work for additional
 * information regarding copyright ownership.
 *
 * This program and the accompanying materials are made available under the
 * terms of the Apache License, Version 2.0 which is available at
 * https://www.apache.org/licenses/LICENSE-2.0.
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * SPDX-License-Identifier: Apache-2.0
 ********************************************************************************/

using System.ComponentModel.DataAnnotations;

namespace DimProcess.Library.DependencyInjection;

public class DimHandlerSettings
{
    [Required(AllowEmptyStrings = false)]
    public string AdminMail { get; set; } = null!;

    [Required]
    public Guid RootDirectoryId { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string AuthUrl { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string ClientidCisCentral { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string ClientsecretCisCentral { get; set; } = null!;

    [Required(AllowEmptyStrings = false)]
    public string EncryptionKey { get; set; } = null!;
}
